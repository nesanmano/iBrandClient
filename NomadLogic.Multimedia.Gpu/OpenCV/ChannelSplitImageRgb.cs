using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using JetBrains.Annotations;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.Multimedia.Gpu.OpenCV
{
    /// <summary>
    /// Classe qui prépare un fichier d'image en y préparant des objets propres à OpenCv basé sur l'image.
    /// Cela inclu une séparation des canaux de couleurs, une préparation des valeurs alpha de l'image (si existant).
    /// Toute les opération sont en BGR ou BGRA selon le fichier d'origine. Comme la quasi totalité des images sont stockée sur disque
    /// en format RGB, cette méthode regroupe les méthode pour exporter les matrices de canaux contenues dans ce format.
    /// Les méthode annexes permettent de manipuler le canal alpha, s'il existe. On peut également créé de nouvelle images
    /// en réutilisant les canaux de couleur mais en leur ajoutant un canal d'alpha externe. Ce faisant permet de mieux
    /// traiter l'alpha à part des canaux de couleur. Ce qui facilite certaines opérations, comme les clés chromatiques.
    /// </summary>
    [PublicAPI]
    public class ChannelSplitImageRgb : IDisposable
    {
        /// <summary>
        /// La matrice complète de l'image originale. Tirée d'un fichier, donc en format BGR ou BGRA
        /// </summary>
        public Mat BaseMat { get; private set; }

        /// <summary>
        /// Les matrices séparées de l'image, par canal de couleur, de format BGR ou BGRA selon l'image de base.
        /// </summary>
        public Mat[] BaseImageChannels { get; private set; }

        /// <summary>
        /// Une matrice complète des informations de couleurs de l'image, en format BGR. Comporte 3 canaux.
        /// </summary>
        public Mat BaseImageBgr { get; private set; }

        /// <summary>
        /// Tableau des valeurs alpha en bytes [0..255] en tableau contiguë.
        /// Peut être NULL si l'image ne contenait pas d'information d'alpha à l'origine.
        /// </summary>
        public byte[] WorkingAlpha { get; protected set; }

        /// <summary>
        /// Référence vers un tableau de travail pour le calcul d'alpha. Permet de réutiliser la même plage mémoire pour le calcul
        /// répété.
        /// </summary>
        internal byte[] TmpAlphaData { get; set; }

        /// <summary>
        /// Méthode d'initialisation de base. Agit comme un constructeur décalé. Contruit les canaux de l'instance via les paramètres.
        /// Fait au préalable un "Dispose" pour nettoyer les canaux précédents.
        /// </summary>
        /// <param name="image">L'image matricielle de base</param>
        /// <param name="newSize"></param>
        public void Init(CvArray<byte> image, Size? newSize)
        {
            Init(image?.Mat, newSize);
        }
        
        /// <summary>
        /// Méthode d'initialisation de base. Agit comme un constructeur décalé. Contruit les canaux de l'instance via les paramètres.
        /// Fait au préalable un "Dispose" pour nettoyer les canaux précédents.
        /// </summary>
        /// <param name="baseImage">L'image matricielle de base</param>
        /// <param name="newSize"></param>
        public void Init(Mat baseImage, Size? newSize)
        {
            Dispose();
            
            if (baseImage == null || baseImage.NumberOfChannels < 3)
            {
                throw new System.Exception("Illegal Matrix. Must have at least 3 channels, and not be null.");
            }

            if (newSize != null)
            {
                var imageResize = MathUtil.GetNewSizeToFitInsideContainer(newSize.Value.Width, newSize.Value.Width, baseImage.Width, baseImage.Height).ToSize();
                CvInvoke.Resize(baseImage, baseImage, imageResize, 0, 0, Inter.Area);
            }

            GenerateRgbMats(baseImage);

            FormatConversion();
        }

        protected void GenerateRgbMats([NotNull] Mat baseImage, bool skipAlpha = false)
        {
            BaseImageBgr?.Dispose();
            BaseImageBgr = null;
            BaseMat?.Dispose();
            BaseMat = null;
            BaseImageChannels = null;
            
            BaseMat = baseImage;
            BaseImageChannels = baseImage.Split();
            
            if (!skipAlpha)
            {
                if (BaseMat.NumberOfChannels == 4)
                {
                    WorkingAlpha = new byte[BaseMat.Rows * BaseMat.Cols];
                    Marshal.Copy(BaseImageChannels[3].DataPointer, WorkingAlpha, 0, WorkingAlpha.Length);
                }
            }

            BaseImageBgr = new Mat(BaseMat.Rows, BaseMat.Cols, BaseMat.Depth, 3);
            var channels = new VectorOfMat();
            channels.Push(BaseImageChannels[0]);
            channels.Push(BaseImageChannels[1]);
            channels.Push(BaseImageChannels[2]);
            CvInvoke.Merge(channels, BaseImageBgr);
        }

        /// Si une classe dérivée à besoin d'une conversion de valeur matricielle, elle peut surcharger cette méthode, qui est exécutée à la fin de
        /// l'initialisation.
        protected virtual void FormatConversion() {} 

        /// <summary>
        /// Créer une image BGRA à partir des données BGR de l'image, avec un nouveau canal de transparence.
        /// </summary>
        /// <returns>Une référence vers la nouvelle image BGRA-byte.</returns>
        public Image<Bgra, byte> GetReferencedImageDataFromSplit()
        {
            var colorChannels = new VectorOfMat();
            colorChannels.Push(BaseImageChannels[0]);
            colorChannels.Push(BaseImageChannels[1]);
            colorChannels.Push(BaseImageChannels[2]);

            var alpha = new Mat(BaseMat.Rows, BaseMat.Cols, DepthType.Cv8U, 1);

            colorChannels.Push(alpha);

            var finalChannels = new Mat(BaseMat.Rows, BaseMat.Cols, DepthType.Cv8U, 4);
            CvInvoke.Merge(colorChannels, finalChannels);

            return finalChannels.ToImage<Bgra, byte>();
        }

        /// <summary>
        /// Créer une nouvelle image OpenCv à partir des données BGR de l'image, en y ajoutant une matrice qui
        /// représente les données alpha calculées.
        /// </summary>
        /// <param name="alphaData">La matrice alpha.</param>
        /// <returns>Une nouvelle référence BGRA-byte.</returns>
        public Image<Bgra, byte> AssembleImageDataFromSplit(Mat alphaData)
        {
            var colorChannels = new VectorOfMat();

            colorChannels.Push(BaseImageChannels[0]);
            colorChannels.Push(BaseImageChannels[1]);
            colorChannels.Push(BaseImageChannels[2]);

            colorChannels.Push(alphaData);

            var finalChannels = new Mat(BaseMat.Rows, BaseMat.Cols, DepthType.Cv8U, 4);
            CvInvoke.Merge(colorChannels, finalChannels);

            return finalChannels.ToImage<Bgra, byte>();
        }

        /// <summary>
        /// Retourne une nouvelle image basé uniquement sur le canal alpha de cette instance.
        /// Il s'agit d'une copie. Et peut retourner null si l'instance ne possède pas d'alpha.
        /// </summary>
        /// <returns>Une image Greyscale du canal alpha de l'instance. Null si pas de canal alpha.</returns>
        public Image<Gray, byte> GetOriginalAlphaImage()
        {
            if (BaseMat == null || BaseImageChannels == null) return null;
            if (BaseImageChannels.Length < 4) return null;

            var alphaMat = new Mat();
            BaseImageChannels[3].CopyTo(alphaMat);
            
            return alphaMat.ToImage<Gray, byte>();
        }
        
        /// <summary>
        /// Retourne une nouvelle image basé uniquement sur le canal alpha temporaire de cette instance.
        /// Il s'agit d'une copie. Et peut retourner null si on n'a pas encore initialisé le canal d'alpha temporaire.
        /// </summary>
        /// <returns>Une image Greyscale du canal alpha de l'instance. Null si pas de canal alpha.</returns>
        public Image<Gray, byte> GetWorkingAlphaImage()
        {
            if (WorkingAlpha == null) return null;
          
            var alphaMat = new Mat(BaseMat.Rows, BaseMat.Cols, DepthType.Cv8U, 1);
            alphaMat.SetTo(WorkingAlpha);
            
            return alphaMat.ToImage<Gray, byte>();
        }

        /// <summary>
        /// Créer une nouvelle image OpenCv à partir des données BGR de l'image, en y ajoutant une matrice qui
        /// représente les données alpha calculées.
        /// </summary>
        /// <param name="alphaData">Le tableau de valeur qui représente l'apha à ajouter.</param>
        /// <returns>Une nouvelle référence vers une image BGRA-byte.</returns>
        public Image<Bgra, byte> AssembleImageDataFromSplit(byte[] alphaData)
        {
            var alpha = new Mat(BaseMat.Rows, BaseMat.Cols, DepthType.Cv8U, 1);
            Marshal.Copy(alphaData, 0, alpha.DataPointer, alphaData.Length);

            return AssembleImageDataFromSplit(alpha);
        }
        
        /// <summary>
        /// Créer une nouvelle image OpenCv à partir des données BGR de l'image, en utilisant le WorkingAlpha.
        /// Sinon, utilise un canal alpha temporaire totalement opaque (valeur maximale).
        /// </summary>
        /// <returns>Une nouvelle référence vers une image BGRA-byte.</returns>
        public Image<Bgra, byte> AssembleImageDataFromSplit()
        {
            var alpha = WorkingAlpha;
            if (alpha != null) return AssembleImageDataFromSplit(alpha);
            
            var mat = new Matrix<byte>(BaseMat.Rows, BaseMat.Cols);
            mat.SetValue(byte.MaxValue);
            alpha = mat.Bytes;

            return AssembleImageDataFromSplit(alpha);
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            BaseMat?.Dispose();
            BaseMat = null;
            BaseImageBgr?.Dispose();
            BaseImageBgr = null;
            if (BaseImageChannels != null)
            {
                foreach (var bic in BaseImageChannels)
                {
                    bic?.Dispose();
                }
                BaseImageChannels = null;
            }
            WorkingAlpha = null;
            TmpAlphaData = null;
        }
    }
}