using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Multimedia.FaceDetection;
using NomadLogic.Multimedia.FaceDetection.Harr;

namespace NomadLogic.Multimedia.XmlTransformation
{
    /// <summary>
    /// Cette classe contient des méthodes statiques utiles pour les principales étapes de transformations d'une image.
    /// </summary>
    [PublicAPI]
    public static class TransformationUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// La machine de configuration de détection faciale.
        /// Présentement, on n'utilise qu'une seule configuration. Il faudrait reviser ce modèle si on veut en utiliser plusieurs.
        /// </summary>
        private static HaarClassifierCascade _haarClassifierCascade;

        /// <summary>
        /// Exécute l'ensemble des opérations sur une image pour calculer le rectangle de modification. l'image ne doit pas encore être modifiée par le
        /// redimensionnement et la rotation donnée en paramètre.
        /// </summary>
        /// <param name="image">L'image originale.</param>
        /// <param name="scale">Optionel. Les paramètres de redimensionnement, étant un tableau de dimensions, dont l'une ou l'autre peut être zéro.
        /// Auquel cas la dimension de valeur zéro sera calculé en prorata de l'autre dimension. Si null, on prend les dimensions de l'image.</param>
        /// <param name="rotation">Optionnel. La rotation, en degrés, à effectuer. Si null, il n'ya pas de rotation (degré de zéro).</param>
        /// <returns>Le rectangle de modification.</returns>
        public static Rectangle ExecuteFullFacialRecognition(Bitmap image, int[] scale, double? rotation)
        {
           return DetectRectangleCoordinatesForModifiedFaceImage(DetectFaces(image), image, scale, rotation);
        }

        public static Rectangle DetectRectangleCoordinatesForModifiedFaceImage(List<AvgComp> detectionList, Bitmap imageOriginale, int[] scale, double? rotation)
        {
            Rectangle? param = null;
            if ((detectionList != null && detectionList.Count != 0))
            {
                param = GetFaceRectangle(detectionList, imageOriginale);
            }

            return DetectRectangleCoordinatesForModifiedFaceImage(param, imageOriginale, scale, rotation);
        }

        /// <summary>
        /// Caclcule le centre d'une image qui sera modifiée ultérieurement par une rotation et un redimensionnement, tout deux optionnels.
        /// <br/>Les transformations retournées dans le rectangle résultat sont les modifications pour obtenir le centre du visage sur une image modifiée
        /// par la rotation et le redimensionnement passé en paramètre. On veut obtenir ici le centre de l'image aux coordonnées 0,0.
        /// </summary>
        /// <param name="rectangleFace">Le rectangle du visage. Si null, on fait un calcul du rectangle selon l'ensemble de l'image.</param>
        /// <param name="imageOriginale">L'image originale, pas encore modifiée, contenant le visage à analyser.</param>
        /// <param name="scale">Optionel. Les paramètres de redimensionnement, étant un tableau de dimensions, dont l'une ou l'autre peut être zéro.
        /// Auquel cas la dimension de valeur zéro sera calculé en prorata de l'autre dimension. Si null, on prend les dimensions de l'image.</param>
        /// <param name="rotation">Optionnel. La rotation, en degrés, à effectuer. Si null, il n'ya pas de rotation (degré de zéro).</param>
        /// <returns>Le rectangle de modification.</returns>
        public static Rectangle DetectRectangleCoordinatesForModifiedFaceImage(Rectangle? rectangleFace, Bitmap imageOriginale, int[] scale, double? rotation)
        {
            var nonRotatedCentreX = 0;
            var nonRotatedCentreY = 0;
            var scaledHeight = 0;
            var scaledWidth = 0;
            var modifiedRectangle = new Rectangle();

            // Si on veut utiliser 
            if (rectangleFace != null)
            {
                // calcul de la hauteur de l'image qu'on doit avoir pour que la
                // face soit de la bonne grosseur
                if (imageOriginale != null) {
                    scaledHeight = (scale == null || scale[1] == 0) ? imageOriginale.Height : scale[1];
                    // calcul de la largeur pour pas déformer l'image
                    scaledWidth = scaledHeight * imageOriginale.Width / imageOriginale.Height;

                    // calcul du centre de la face redimensionné
                    nonRotatedCentreX = (rectangleFace.Value.X + rectangleFace.Value.Width / 2) * scaledWidth / imageOriginale.Width;
                    nonRotatedCentreY = (rectangleFace.Value.Y + rectangleFace.Value.Height / 2) * scaledHeight / imageOriginale.Height;
                }
            }
            else
            {
                if (imageOriginale != null) {
                    scaledHeight = (scale == null || scale[1] == 0) ? imageOriginale.Height : scale[1];
                    // calcul du scaling de l'image pour qu'elle soit de la bonne grosseur
                    scaledWidth = scaledHeight * imageOriginale.Width / imageOriginale.Height;

                    // position de l'ancien centre de l'image sur la nouvelle image
                    nonRotatedCentreX = imageOriginale.Width * scaledWidth / imageOriginale.Width / 2;
                    nonRotatedCentreY = imageOriginale.Height * scaledHeight / imageOriginale.Height / 2;
                }
            }

            var rot = rotation ?? 0;

            //calcul du centre de l'image (afce ou autre)
            modifiedRectangle.X = MathUtil.GetRotationWidth(nonRotatedCentreX, nonRotatedCentreY, rot);
            modifiedRectangle.Y = MathUtil.GetRotationHeight(nonRotatedCentreX, nonRotatedCentreY, rot);

            // on calcul les valeurs selon la rotation de l'image
            modifiedRectangle.Width = MathUtil.GetRotationWidth(scaledWidth, scaledHeight, rot);
            modifiedRectangle.Height = MathUtil.GetRotationHeight(scaledWidth, scaledHeight, rot);

            return modifiedRectangle;
        }

        /// <summary>
        /// Accède à l'unique configuration de détection faciale que l'on possède pour l'instant.
        /// <br/>Il s'agit d'une initialisation fainéante, qui se fait la première fois que l'on veut accéder à cette information.
        /// </summary>
        /// <returns>L'objet de configuration Haar obtenu grâce à la lecture d'un fichier de configuration statique dans
        /// le répertoire d'installation.</returns>
        public static HaarClassifierCascade GetHaarClassifierCascade()
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

        /// <summary>
        /// Permet d'avoir un rectangle entourant la face à partir d'une liste de faces possibles trouvés
        /// avec la détection de face de OpenCV avec neighbours à 0. On fait des statistiques pour éliminer 
        /// les rectangles extrêmes qui sont seuls dans leur coin.
        /// </summary>
        /// <param name="list">Liste des résultats de détection faciale.</param>
        /// <param name="image">L'iamge à analyser</param>
        /// <returns>Le rectangle entourant le visage.</returns>
        public static Rectangle GetFaceRectangle(List<AvgComp> list, Bitmap image)
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

            if (list != null) {
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
                    if (rec == null) continue;
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
            }

            var centerX = x + width / 2;
            var centerY = y + height / 2;
            var upFound = false;

            for (var i = centerY; !upFound; i--)
            {
                if (image != null && ((image.GetPixel(centerX, i).A != 0) && i != 0)) continue;
                height += y - i;
                y = i;
                upFound = true;
            }

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Exécute une détection facile avec un carré Haar de 125x125, et un facteur de grossissement de 1.075.
        /// </summary>
        /// <param name="image">L'image à analyser.</param>
        /// <returns>Liste des rectangle de visage détectés.</returns>
        private static List<AvgComp> DetectFaces(Bitmap image)
        {
            return DetectFaces(image, 125, 125, 1.075);
        }

        /// <summary>
        /// Exécute une détection faciale avec une taille de détection précise, et un facteur de grossissement précis.
        /// </summary>
        /// <param name="image">L'image à analyser.</param>
        /// <param name="sizeX">Taille en X du carré de détection Haar.</param>
        /// <param name="sizeY">Taille en Y du carré de détection Haar.</param>
        /// <param name="scaleFactor">Facteur de grossissement Haar.</param>
        /// <returns>Liste des rectangle de visage détectés.</returns>
        public static List<AvgComp> DetectFaces(Bitmap image, int sizeX, int sizeY, double scaleFactor)
        {
            return FaceDetectionUtil.DetectFaces(image, GetHaarClassifierCascade(), new HarrSize(sizeX, sizeY), scaleFactor);
        }
    }
}
