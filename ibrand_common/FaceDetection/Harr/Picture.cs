using System;
using System.Drawing;

namespace NomadLogic.iBrand.Transformation.FaceDetection.Harr
{
    public class Picture
    {
        private const float CR = .2989f;
	    private const float CG = .5870f;
	    private const float CB = .1140f;

        private readonly int[][] _matrix;

        /**
         * Constructeur
         */
        public Picture() {

        }

        /**
         * Constructeur
         * @param width La largeur de l'image
         * @param height La hauteur de l'image
         */
        public Picture(int width, int height) {
            Width = width;
            Height = height;
            _matrix = new int[height][];
            for (var x = 0; x < _matrix.Length; x++)
            {
                _matrix[x] = new int[width];
            }
        }

        /**
         * Avoir le pixel
         * @param y la position en y du point
         * @param x la position en x du point
         * @return la valeur de la couleur
         */
        public int Get(int y, int x)
        {
            return _matrix[y][x];
        }

        /**
         * Mettre à jour la valeur d'un point
         * @param y la position en y du point
         * @param x la position en x du point
         * @param valeur la nouvelle valeur
         */
        public void Set(int y, int x, int valeur)
        {
            _matrix[y][x] = valeur;
        }

        /**
         * Avoir la valeur d'un point
         * @param p le point désiré
         * @return la valeur du point
         */
        public int Get(HaarPoint p) {
    	    return _matrix[p.Y][p.X];
        }

        /**
         * Mettre à jour la valeur d'un point
         * @param p le point à mettre à jour
         * @param valeur la nouvelle valeur
         */
        public void Set(HaarPoint p, int valeur) {
    	    _matrix[p.Y][p.X]= valeur;
        }

        /**
         * Avoir une picture grayscaled à partir d'une BufferedImage
         * @param image l'image à transformer
         * @return l'image en greyscale
         */
        public static Picture FromImageGrayScale(Bitmap image)
        {
            var pic = new Picture(image.Width, image.Height);
            var width = image.Width & (~3);
            int y;

            for (y = 0; y < image.Height; y++)
            {
                int x;
                for (x = 0; x < width; x += 4)
                {
                    var c = image.GetPixel(x, y).ToArgb();
                    pic._matrix[y][x] = (int)(GetRouge(c) * CR + GetVert(c) * CG + GetBleu(c) * CB);

                    c = image.GetPixel(x + 1, y).ToArgb();
                    pic._matrix[y][x + 1] = (int)(GetRouge(c) * CR + GetVert(c) * CG + GetBleu(c) * CB);


                    c = image.GetPixel(x + 2, y).ToArgb();

                    pic._matrix[y][x + 2] = (int)(GetRouge(c) * CR + GetVert(c) * CG + GetBleu(c) * CB);


                    c = image.GetPixel(x + 3, y).ToArgb();

                    pic._matrix[y][x + 3] = (int)(GetRouge(c) * CR + GetVert(c) * CG + GetBleu(c) * CB);

                }

                for (; x < image.Width; x++)
                {
                    var c = image.GetPixel(x, y).ToArgb();

                    pic._matrix[y][x] = (int)(GetRouge(c) * CR + GetVert(c) * CG + GetBleu(c) * CB);

                }
            }

            return pic;
        }

        /**
         * Avoir une image en Picture pour le traitement Haar
         * @param image l'image
         * @return l'image en format pour le traitement Haar
         */
        public static Picture FromImage(Bitmap image)
        {
            var pic = new Picture(image.Width, image.Height);

            for (int y = 0, i = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++, i++)
                {
                    pic.Set(y, x, image.GetPixel(x, y).ToArgb());
                }
            }

            return pic;
        }

        /**
         * Pas implémenté
         */
        public void EqualizeHistogram()
        {
            throw new NotImplementedException();
        }

        /**
         * Avoir une bufferedImage à partir de cette picture.
         * pas implémenté
         * @return rien
         */
        public Bitmap ToImage()
        {
            throw new NotImplementedException();
        }

	    /**
	     * Avoir la valeur R de la couleur
	     * @param pixel le pixel
	     * @return la valeur du rouge du pixel
	     */
	    private static int GetRouge(int pixel) {
		    return ((pixel >> 16) & 0xff);
	    }

	    /**
	     * Avoir la valeur G de la couleur
	     * @param pixel le pixel
	     * @return la valeur du vert du pixel
	     */
	    private static int GetVert(int pixel) {
		    return ((pixel >> 8) & 0xff);
	    }

	    /**
	     * Avoir la valeur B de la couleur
	     * @param pixel le pixel
	     * @return la valeur du bleu du pixel
	     */
	    private static int GetBleu(int pixel) {
		    return (pixel & 0xff);
	    }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
