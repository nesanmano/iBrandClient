using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NomadLogic.iBrand.Transformation.FaceDetection.Harr;
using System.Xml;
using NomadLogic.iBrand.Transformation.FaceDetection;
using NomadLogic.iBrand.Common.Util;
using NLog;
using NomadLogic.iBrand.Common.Respondent;
using NomadLogic.iBrand.Common.Services;
using NomadLogic.iBrand.Common.Util.Math;
using NomadLogic.iBrand.Transformation.Video.Ffmpeg;

namespace NomadLogic.iBrand.Transformation.Layer
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class PhotoLayer : FileLayer
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected bool _face_detection;
        protected bool _green_screen;
        protected Dictionary<string, float> _green_screen_filter_params;

        protected HaarClassifierCascade _haar_classifier_cascade;
        protected bool _recalculer_face_detection = true;
        protected Rectangle _rectangle_face;
        protected const float MAX_GSH_WIDTH = 1504.0f;
        protected const float MAX_GSH_HEIGHT = 1000.0f;
        
        protected int _modified_width = -1;
        protected int _modified_height = -1;
        protected int _modified_x = -1;
        protected int _modified_y = -1;

        public PhotoLayer(PhotoLayer original)
            : base(original)
        {
            _face_detection = original._face_detection;
            _green_screen = original._green_screen;

            if (original._green_screen_filter_params != null)
            {
                var iter = original._green_screen_filter_params.GetEnumerator();
                while (iter.MoveNext())
                {
                    _green_screen_filter_params.Add(iter.Current.Key, iter.Current.Value);
                }
                iter.Dispose();
            }

            if (original._haar_classifier_cascade != null)
            {
                _haar_classifier_cascade = original._haar_classifier_cascade;
            }
            _recalculer_face_detection = original._recalculer_face_detection;
            _rectangle_face = original._rectangle_face;

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

            var greenScreen = reader.GetAttribute("green_screen");
            if (greenScreen == "true")
                GreenScreen = true;

            var square = reader.GetAttribute("square");
            if (square == "true")
                Square = true;

            var faceDetection = reader.GetAttribute("face_detection");
            if (faceDetection == "true")
                FaceDetection = true;

            var faceSize = reader.GetAttribute("face_size");
            float floatSizeVal;
            if (float.TryParse(faceSize, out floatSizeVal))
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
                            _green_screen_filter_params = new Dictionary<string, float>();
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
                                            _green_screen_filter_params.Add(paramName, paramValue);
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
            get { return _face_detection; }
            set { _face_detection = value; SettingsHaveChanged(); }
        }

        public Rectangle RectangleFace
        {
            get { return _rectangle_face; }
            set { _rectangle_face = value; SettingsHaveChanged(); }
        }

        public HaarClassifierCascade GetHaarClassifierCascade(IRespondent respondent)
        {
            if (_haar_classifier_cascade != null) return _haar_classifier_cascade;

            var paramsFace = DirectoryUtil.GetCurrentApplicationInstallAssetsDir() + "face_detection" + Path.DirectorySeparatorChar + "haarcascade_frontalface_alt2.xml";

            if (!File.Exists(paramsFace))
            {
                log.Error("Could not find Haar file: " + paramsFace);
                return null; // Pas d'autres contre-mesure pour l'instant.
            }

            try
            {
                var reader = new XmlTextReader(paramsFace);
                _haar_classifier_cascade = HaarClassifierCascade.Parse(reader);
                reader.Close();
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not parse XML file.");
                return null; // Pas d'autres contre-mesure pour l'instant.
            }
            return _haar_classifier_cascade;
        }

        public bool GreenScreen
        {
            get { return _green_screen; }
            set { _green_screen = value; SettingsHaveChanged(); }
        }

        public bool Square
        {
            get { return _square; }
            set { _square = value; SettingsHaveChanged(); }
        }

        public Dictionary<string, float> GreenScreenFilterParams
        {
            get { return _green_screen_filter_params; }
            set { _green_screen_filter_params = value; SettingsHaveChanged(); }
        }

        public override string GetFile(IRespondent respondent)
        {
            return respondent.GetResponse(QuestionNo);
        }

        public override void GetImage(IRespondent respondent)
        {
            if (_final_image != null) return;

            if (_file_image == null)
            {
                var file = GetFile(respondent);
                
                if (file == null || !File.Exists(file))
                    throw new FileNotFoundException("file " + file + " not found");

                if (Exif)
                {
                    ImageUtil.RotateJpegToMatchExifData(file);
                }
                
                try
                {
                    _file_image = new Bitmap(file);
                    _file_image.SetResolution(300, 300);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException("Could not open image file: " + file, e);
                }

                if (Square)
                {
                    _file_image = ImageUtil.ResizeImageToSquare(_file_image);
                }
            }

            var image = new Bitmap(_file_image);
            image.SetResolution(300, 300);
            _file_image.Dispose();
            if (_green_screen)
            {
                if (_face_detection && (image.Width > MAX_GSH_WIDTH || image.Height > MAX_GSH_HEIGHT))
                {
                    var ratioWidth = MAX_GSH_WIDTH / image.Width;
                    var ratioHeight = MAX_GSH_HEIGHT / image.Height;
                    var changeRatio = Math.Max(ratioWidth, ratioHeight);

                    var modWidth = (int)(image.Width * changeRatio);
                    var modHeight = (int)(image.Height * changeRatio);

                    image = ImageUtil.GetScaledInstance(image, modWidth, modHeight);

                    _modified_width = modWidth;
                    _modified_height = modHeight;
                }

                var tmpFileOut = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");
                var tmpFileIn = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".png");

                var conf = ServicesContainer.GetService<NomadConfiguration>();

                var filter = File.Exists(FfmpegChromaKeyInfo.DefaultConfigFileLocation)
                    ? FileUtil.ReadJsonFile<FfmpegChromaKeyInfo>(
                        FfmpegChromaKeyInfo.DefaultConfigFileLocation)
                    : FfmpegChromaKeyInfo.NewInstanceChromaKeyBlue();

                ImageUtil.SaveToImageFileWithOptions(image, ImageFormat.Png, 9, tmpFileIn);
                
                FfmpegUtil.ChromakeyImage(conf.FfmpegLocation, tmpFileIn, tmpFileOut, filter);

                _final_image = ImageUtil.LoadBitmapFromFile(tmpFileOut);
                _final_image.SetResolution(300, 300);
                
                if (_face_detection)
                {
                    List<AvgComp> detectionList = null;
                    if (_recalculer_face_detection)
                    {
                        detectionList = Transformation.FaceDetection.FaceDetection.DetectFaces(_final_image,
                            GetHaarClassifierCascade(respondent), new FaceDetection.Harr.Size(125, 125), 1.075); // 1.025
                    }
                    AdjustImageProperties(detectionList);
                }
                else // Green Screen, mais sans détection faciale.
                {
                    _modified_width = Width;
                    _modified_height = Height;
                    _modified_x = X;
                    _modified_y = Y;
                }
            }
            else
            {
                _final_image = new Bitmap(image.Width, image.Height, image.PixelFormat);
                _final_image.SetResolution(300, 300);
                var g = Graphics.FromImage(_final_image);

                g.DrawImage(image, new Point(0, 0));
                g.Dispose();

                _modified_width = Width;
                _modified_height = Height;
                _modified_x = X;
                _modified_y = Y;
            }

            image.Dispose();

            AdditionalFilters(respondent);

            if (Scale != null || _nonScaleFactor != 1f)
            {
                Scale = new[] { _modified_width, _modified_height };
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
                    g2D.DrawImageUnscaled(_final_image, _modified_x, _modified_y, _modified_width, _modified_height);
                    g2D.Flush();
                    g2D.Dispose();
                    break;
                case BLENDING_MULTIPLY:
                    DrawMultiply(image, _final_image, _modified_x, _modified_y);
                    break;
            }
        }

        private void AdjustImageProperties(List<AvgComp> detectionList)
        {
            int nonRotatedCentreX;
            int nonRotatedCentreY;
            int scaledHeight;
            int scaledWidth;

            var image = new Bitmap(_final_image);
            image.SetResolution(300, 300);

            // s'il y a des faces
            if ((detectionList != null && detectionList.Count != 0) || (!_recalculer_face_detection))
            {
                //imageFinale = imageTransforme;
                //On traite les rectangles en résultats pour avoir un seul rectangle
                if (_recalculer_face_detection)
                    _rectangle_face = GetFaceRectangle(detectionList, image);

                // Ratio de redimensionnement pour grossir ou rapetisser l'image selon le rectangle détecté.
                var percResize = (FaceSize / _rectangle_face.Width);

                // calcul de la hauteur de l'image qu'on doit avoir pour que la
                // face soit de la bonne grosseur
                scaledHeight = (int)(((Scale == null || Scale[1] == 0) ? Height : Scale[1]) * percResize);
                // calcul de la largeur pour pas déformer l'image
                scaledWidth = (int)(scaledHeight * image.Width / (float)image.Height);

                // calcul du centre de la face scalé
                nonRotatedCentreX = (_rectangle_face.X + _rectangle_face.Width / 2) * scaledWidth / image.Width;
                nonRotatedCentreY = (_rectangle_face.Y + _rectangle_face.Height / 2) * scaledHeight / image.Height;
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
            _modified_width = MathUtil.GetRotationWidth(scaledWidth, scaledHeight, Rotation);
            _modified_height = MathUtil.GetRotationHeight(scaledWidth, scaledHeight, Rotation);

            _modified_x = X - centerX;
            _modified_y = Y - centerY;

            _final_image.Dispose();
            _final_image = image;

        }

        public bool RecalculerFaceDetection
        {
            get { return _recalculer_face_detection; }
            set { _recalculer_face_detection = value; SettingsHaveChanged(); }
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
        public bool Exif { get { return _exif; } set { _exif = value; SettingsHaveChanged(); } }

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

            _modified_x = -1;
            _modified_y = -1;
            _modified_width = -1;
            _modified_height = -1;
        }
    }
}
