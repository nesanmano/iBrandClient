using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Multimedia.FaceDetection;
using NomadLogic.Multimedia.FaceDetection.Harr;

namespace NomadLogic.Multimedia.XmlTransformation.Layer
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    [PublicAPI]
    public class PhotoLayer : FileLayer
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected bool _faceDetection;
        protected bool _greenScreen;
        protected Dictionary<string, float> _greenScreenFilterParams;

        protected HaarClassifierCascade _haarClassifierCascade;
        protected bool _recalculerFaceDetection = true;
        protected Rectangle _rectangleFace;
        protected const float MAX_GSH_WIDTH = 1504.0f;
        protected const float MAX_GSH_HEIGHT = 1000.0f;
        
        protected int _modifiedWidth = -1;
        protected int _modifiedHeight = -1;
        protected int _modifiedX = -1;
        protected int _modifiedY = -1;

        public PhotoLayer(PhotoLayer original)
            : base(original)
        {
            _faceDetection = original._faceDetection;
            _greenScreen = original._greenScreen;

            if (original._greenScreenFilterParams != null)
            {
                var iter = original._greenScreenFilterParams.GetEnumerator();
                while (iter.MoveNext())
                {
                    _greenScreenFilterParams.Add(iter.Current.Key, iter.Current.Value);
                }
                iter.Dispose();
            }

            if (original._haarClassifierCascade != null)
            {
                _haarClassifierCascade = original._haarClassifierCascade;
            }
            _recalculerFaceDetection = original._recalculerFaceDetection;
            _rectangleFace = original._rectangleFace;

            Exif = original.Exif;

            EnableSettingChangeEventOnScaleChange = false;
        }

        public PhotoLayer(int x, int y, int[] scale)
            : base(x, y, scale)
        {
            EnableSettingChangeEventOnScaleChange = false;
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public PhotoLayer(int x, int y, int[] scale, XmlReader reader)
            : this(x, y, scale)
        {
            var questionNo = reader.GetAttribute("question_no");
            if (questionNo != null)
                QuestionNo = int.Parse(questionNo);

            GreenScreen = bool.Parse(reader.GetAttribute("green_screen") ?? "false");
            Square = bool.Parse(reader.GetAttribute("square") ?? "false"); ;
            FaceDetection = bool.Parse(reader.GetAttribute("face_detection") ?? "false");

            var faceSize = reader.GetAttribute("face_size");
            if (float.TryParse(faceSize, out var floatSizeVal))
            {
                FaceSize = floatSizeVal;
            }
            
            var exif = reader.GetAttribute("look_for_exif");
            if (exif == "true")
                Exif = true;

            if (reader.IsEmptyElement) return;
            var layerFinished = false;

            while (!layerFinished)
            {
                reader.Read();
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals("green_screen_config"))
                        {
                            _greenScreenFilterParams = new Dictionary<string, float>();
                            var paramsFinished = false;

                            while (!paramsFinished)
                            {
                                reader.Read();
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (reader.Name.Equals("green_screen_parameter"))
                                        {
                                            var paramName = reader.GetAttribute("name");
                                            var value = reader.GetAttribute("value");
                                            var paramValue = float.Parse(value);
                                            _greenScreenFilterParams.Add(paramName, paramValue);
                                        }
                                        break;

                                    case XmlNodeType.EndElement:
                                        if (reader.Name.Equals("green_screen_config"))
                                        {
                                            paramsFinished = true;
                                        }
                                        break;
                                }
                            }
                        }

                        break;
                    case XmlNodeType.EndElement:
                        if (reader.Name.Equals("layer"))
                        {
                            layerFinished = true;
                        }
                        break;
                }
            }
        }

        public bool FaceDetection
        {
            get => _faceDetection;
            set { _faceDetection = value; SettingsHaveChanged(); }
        }

        public Rectangle RectangleFace
        {
            get => _rectangleFace;
            set { _rectangleFace = value; SettingsHaveChanged(); }
        }

        public HaarClassifierCascade GetHaarClassifierCascade()
        {
            if (_haarClassifierCascade != null) return _haarClassifierCascade;

            var paramsFace = DirectoryUtil.GetLocalNomadLogicAssetsDir() + "face_detection" + Path.DirectorySeparatorChar + "haarcascade_frontalface_alt2.xml";

            if (!File.Exists(paramsFace))
            {
                log.Error("Could not find Haar file: " + paramsFace);
                return null; // Pas d'autres contre-mesure pour l'instant.
            }

            try
            {
                var reader = new XmlTextReader(paramsFace);
                _haarClassifierCascade = HaarClassifierCascade.Parse(reader);
                reader.Close();
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not parse XML file.");
                return null; // Pas d'autres contre-mesure pour l'instant.
            }
            return _haarClassifierCascade;
        }

        public bool GreenScreen
        {
            get => _greenScreen;
            set { _greenScreen = value; SettingsHaveChanged(); }
        }

        public bool Square
        {
            get => _square;
            set { _square = value; SettingsHaveChanged(); }
        }

        public Dictionary<string, float> GreenScreenFilterParams
        {
            get => _greenScreenFilterParams;
            set { _greenScreenFilterParams = value; SettingsHaveChanged(); }
        }

        public override string GetFile(IRespondent respondent)
        {
            return respondent.GetAnswerText(QuestionNo);
        }

        public override void GetImage(IRespondent respondent)
        {
            if (_finalImage != null) return;

            if (_fileImage == null)
            {
                var file = GetFile(respondent);
                
                if (file == null || !File.Exists(file))
                    throw new FileNotFoundException("file " + file + " not found");

                if (Exif)
                {
                    BitmapExtentions.MatchExifData(file);
                }
                
                try
                {
                    _fileImage = FileUtil.LoadBitmap(file);
                    _fileImage.SetResolution(300, 300);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException("Could not open image file: " + file, e);
                }

                if (Square)
                {
                    var tmp = _fileImage.ResizeToSquare();
                    _fileImage.Dispose();
                    _fileImage = tmp;
                }
            }

            var image = new Bitmap(_fileImage);
            image.SetResolution(300, 300);
            _fileImage.Dispose();
            
            // TODO Ce système ne fait plus le green screen via OpenGL.
            // Vaut mieux faire une extension de cette classe dans le bon sous-projet si on veut ressuciter ce genre de système.
            /*
            if (_greenScreen)
            {
                if (_faceDetection && (image.Width > MAX_GSH_WIDTH || image.Height > MAX_GSH_HEIGHT))
                {
                    var ratioWidth = MAX_GSH_WIDTH / image.Width;
                    var ratioHeight = MAX_GSH_HEIGHT / image.Height;
                    var changeRatio = Math.Max(ratioWidth, ratioHeight);

                    var modWidth = (int)(image.Width * changeRatio);
                    var modHeight = (int)(image.Height * changeRatio);

                    image = image.Scale(modWidth, modHeight);

                    _modifiedWidth = modWidth;
                    _modifiedHeight = modHeight;
                }

                var tmpFileOut = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
                var tmpFileIn = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
                
                image.Save(ImageFormat.Png, 9, tmpFileIn);

                ChromaKeyManager.ChromaKey(tmpFileIn, tmpFileOut);

                _finalImage = FileUtil.LoadBitmap(tmpFileOut);
                _finalImage.SetResolution(300, 300);
                
                if (_faceDetection)
                {
                    List<AvgComp> detectionList = null;
                    if (_recalculerFaceDetection)
                    {
                        detectionList = FaceDetectionUtil.DetectFaces(_finalImage, GetHaarClassifierCascade(), new HarrSize(125, 125), 1.075);
                    }
                    AdjustImageProperties(detectionList);
                }
                else // Green Screen, mais sans détection faciale.
                {
                    _modifiedWidth = Width;
                    _modifiedHeight = Height;
                    _modifiedX = X;
                    _modifiedY = Y;
                }
            }
            else
            {
            */
                _finalImage = new Bitmap(image.Width, image.Height, image.PixelFormat);
                _finalImage.SetResolution(300, 300);
                var g = Graphics.FromImage(_finalImage);

                g.DrawImageUnscaled(image, new Point(0, 0));
                g.Dispose();

                _modifiedWidth = Width;
                _modifiedHeight = Height;
                _modifiedX = X;
                _modifiedY = Y;
            /*
            }
            */

            image.Dispose();

            AdditionalFilters(respondent);

            if (Scale != null || _nonScaleFactor != 1f)
            {
                Scale = new[] { _modifiedWidth, _modifiedHeight };
            }
        }

        protected virtual void AdditionalFilters(IRespondent respondent)
        {
            // rien pour cette classe.
        }

        public override void Draw(Bitmap image, IRespondent respondent)
        {
            GetBitmapFromFile(respondent);

            switch (_blendingMode)
            {
                case BLENDING_NORMAL:
                    var g2D = Graphics.FromImage(image);
                    g2D.DrawImageUnscaled(_finalImage, _modifiedX, _modifiedY, _modifiedWidth, _modifiedHeight);
                    g2D.Flush();
                    g2D.Dispose();
                    break;
                case BLENDING_MULTIPLY:
                    DrawMultiply(image, _finalImage, _modifiedX, _modifiedY);
                    break;
            }
        }

        private void AdjustImageProperties(List<AvgComp> detectionList)
        {
            int nonRotatedCentreX;
            int nonRotatedCentreY;
            int scaledHeight;
            int scaledWidth;

            var image = new Bitmap(_finalImage);
            image.SetResolution(300, 300);

            // s'il y a des faces
            if ((detectionList != null && detectionList.Count != 0) || (!_recalculerFaceDetection))
            {
                //imageFinale = imageTransforme;
                //On traite les rectangles en résultats pour avoir un seul rectangle
                if (_recalculerFaceDetection)
                    _rectangleFace = GetFaceRectangle(detectionList, image);

                // Ratio de redimensionnement pour grossir ou rapetisser l'image selon le rectangle détecté.
                var percResize = (FaceSize / _rectangleFace.Width);

                // calcul de la hauteur de l'image qu'on doit avoir pour que la
                // face soit de la bonne grosseur
                scaledHeight = (int)(((Scale == null || Scale[1] == 0) ? Height : Scale[1]) * percResize);
                // calcul de la largeur pour pas déformer l'image
                scaledWidth = (int)(scaledHeight * image.Width / (float)image.Height);

                // calcul du centre de la face scalé
                nonRotatedCentreX = (_rectangleFace.X + _rectangleFace.Width / 2) * scaledWidth / image.Width;
                nonRotatedCentreY = (_rectangleFace.Y + _rectangleFace.Height / 2) * scaledHeight / image.Height;
            }
            else
            {

                scaledHeight = (Scale == null || Scale[1] == 0) ? Height : Scale[1];
                // calcul du scaling de l'image pour qu'elle soit de la bonne grosseur
                scaledWidth = scaledHeight * image.Width / image.Height;

                // position de l'ancien centre de l'image sur la nouvelle image
                nonRotatedCentreX = image.Width * scaledWidth / image.Width / 2;
                nonRotatedCentreY = image.Height * scaledHeight / image.Height / 2;
            }

            //calcul du centre de l'image (face ou autre)
            var centerX = MathUtil.GetRotationWidth(nonRotatedCentreX, nonRotatedCentreY, Rotation);
            var centerY = MathUtil.GetRotationHeight(nonRotatedCentreX, nonRotatedCentreY, Rotation);

            // on calcul les valeurs selon la rotation de l'image
            _modifiedWidth = MathUtil.GetRotationWidth(scaledWidth, scaledHeight, Rotation);
            _modifiedHeight = MathUtil.GetRotationHeight(scaledWidth, scaledHeight, Rotation);

            _modifiedX = X - centerX;
            _modifiedY = Y - centerY;

            _finalImage.Dispose();
            _finalImage = image;

        }

        /**
         * Permet d'avoir un rectangle entourant la face à partir d'une liste
         * de faces possibles trouvé avec la détection de face de OpenCV avec
         * neighbours à 0.  On fait des statistiques pour éliminer les rectangles
         * extrèmes qui sont seul dans leur coin.
         * @param list Liste de rectangles retournés par l'algorithme de OpenCV
         * @param image L'image où on doit identifier la face
         * @return Le rectangle entourant la face.
         */
        protected Rectangle GetFaceRectangle(List<AvgComp> list, Bitmap image)
        {
            var x = 0;
            var y = 0;
            var width = 0;
            var height = 0;

            // Les données des rectangles séparés pour qu'on puisse trouver
            // les carrés qui ne font probablement pas parti de la face
            // pour avoir une moyenne plus juste
            var centersX = new List<int>();
            var centersY = new List<int>();
            var positionX = new List<int>();
            var positionY = new List<int>();
            var widths = new List<int>();
            var heights = new List<int>();

            foreach (var face in list.Select(t => t.Rectangle))
            {
                centersX.Add(face.X + face.Width / 2);
                centersY.Add(face.Y + face.Height / 2);
                positionX.Add(face.X);
                positionY.Add(face.Y);
                widths.Add(face.Width);
                heights.Add(face.Height);
            }
            // déclaration des boites à moustache (boxplot) qui vont nous permettre
            // de trouver si les données sont correctes
            var boxCenter = new BoxPlot2D(centersX, centersY);
            var boxPosition = new BoxPlot2D(positionX, positionY);
            var boxSizes = new BoxPlot2D(widths, heights);

            for (var j = 0; j < list.Count; j++)
            {
                if (!boxCenter.IsOutlier(centersX[j], centersY[j]) && !boxPosition.IsOutlier(positionX[j], positionY[j]) &&
                    !boxSizes.IsOutlier(widths[j], heights[j])) continue;
                // Si une des données du carré n'a pas rapport, on l'enlève
                list.RemoveAt(j);
                centersX.RemoveAt(j);
                centersY.RemoveAt(j);
                positionX.RemoveAt(j);
                positionY.RemoveAt(j);
                widths.RemoveAt(j);
                heights.RemoveAt(j);
                j--;
            }

            // calcul de la moyenne pour tous les paramètres
            foreach (var rec in list)
            {
                x += rec.Rectangle.X;
                y += rec.Rectangle.Y;
                width += rec.Rectangle.Width;
                height += rec.Rectangle.Height;
            }

            if (list.Count != 0)
            {
                x /= list.Count;
                y /= list.Count;
                width /= list.Count;
                height /= list.Count;
            }

            var centerX = x + width / 2;
            var centerY = y + height / 2;
            var upFound = false;

            for (var i = centerY; !upFound; i--)
            {
                if ((image.GetPixel(centerX, i).A == 0) || i == 0)
                {
                    height += y - i;
                    y = i;
                    upFound = true;
                }
            }
            return new Rectangle(x, y, width, height);
        }

        private bool _exif;
        private bool _square;

        /// <summary>
        /// Indique si on doit gérer les infos EXIF d'une image.
        /// </summary>
        public bool Exif { 
            get => _exif;
            set { _exif = value; SettingsHaveChanged(); } }

        /// <summary>
        /// Dans le cas d'une détection de visage cette valeur va servir de base ultime pour calculer la taille du visage final
        /// une fois la transformation faciale faite. Elle va même détroner la valeur de Scale.
        /// </summary>
        public float FaceSize { get; set; } = 500f;

        public override AbstractLayer GetCopy()
        {
            return new PhotoLayer(this);
        }

        protected override void OnLayerSettingsChanged()
        {
            base.OnLayerSettingsChanged();

            _modifiedX = -1;
            _modifiedY = -1;
            _modifiedWidth = -1;
            _modifiedHeight = -1;
        }
    }
}
