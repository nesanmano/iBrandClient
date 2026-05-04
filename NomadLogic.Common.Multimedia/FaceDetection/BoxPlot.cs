using System.Collections.Generic;

namespace NomadLogic.Multimedia.FaceDetection
{
    class BoxPlot
    {
        // Intervale interquartile

        public BoxPlot(IEnumerable<int> ensemble)
        {
            Data = new List<int>();
            foreach (var i in ensemble)
            {
                Data.Add(i);
            }
            Data.Sort();

            Min = Data[0];
            Max = Data[Data.Count - 1];
            Quartile1 = Data[Data.Count / 4];
            Median = Data[Data.Count / 2];
            Quartile3 = Data[(Data.Count * 3) / 4];
            Iqr = Quartile3 - Quartile1;
            //leftWhisker = quartile1 - (int)(_IQR * 1.5);
            //rightWhisker = quartile3 + (int)(_IQR * 1.5);
            LeftWhisker = Quartile1 - Iqr;
            RightWhisker = Quartile3 + Iqr;
        }

        /**
         * Savoir si la valeur est exceptionnelle
         * @param value la valeur à vérifier
         * @return vrai si la valeur est à l'extérieur des moustaches, donc
         * si elle est exceptionnelle
         */
        public bool IsOutlier(int value)
        {
            return value < LeftWhisker || value > RightWhisker;
        }

        /**
         * @return the quartile1
         */
        public int Quartile1 { get; set; }

        /**
         * @return the mediane
         */
        public int Median { get; set; }

        /**
         * @return the quartile3
         */
        public int Quartile3 { get; set; }

        /**
         * @return the delta
         */
        public int Delta { get; set; }

        /**
         * @return the min
         */
        public int Min { get; set; }

        /**
         * @return the max
         */
        public int Max { get; set; }

        /**
         * @return the moustacheGauche
         */
        public int LeftWhisker { get; set; }

        /**
         * @return the moustacheDroite
         */
        public int RightWhisker { get; set; }

        /**
         * @return the iQR
         */
        public int Iqr { get; set; }

        /**
         * @return the donnees
         */
        public List<int> Data { get; set; }
    }
}
