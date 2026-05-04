using System.Collections.Generic;

namespace NomadLogic.Multimedia.FaceDetection
{
    class BoxPlot2D
    {
        private readonly BoxPlot _statsX;
        private readonly BoxPlot _statsY;

        public BoxPlot2D(List<int> valuesX, List<int> valuesY)
        {
            _statsX = new BoxPlot(valuesX);
            _statsY = new BoxPlot(valuesY);
        }

        /**
         * Savoir si une valeur est exceptionnelle
         * @param x le x de la valeur
         * @param y le y de la valeur
         * @return vrai si la valeur se démarque des autres en X ou Y
         */
        public bool IsOutlier(int x, int y)
        {
            return _statsX.IsOutlier(x) || _statsY.IsOutlier(y);
        }
    }
}
