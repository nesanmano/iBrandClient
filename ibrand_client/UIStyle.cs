namespace NomadLogic.iBrandDesktop
{
    public class UiStyle
    {
        public static readonly int HorizontalStyle = 1;
        public static readonly int VerticalStyle = 2;
        public static readonly int HorizontalVerticalStyle = 3;//change en fonction de l'angle du terminal.

        private int _textColor;
        private int _borderColor;
        private int _orientation = HorizontalVerticalStyle;

        public UiStyle(int textColor, int borderColor, string orientation)
        {
            _textColor = textColor;
            _borderColor = borderColor;

            SetOrientation(orientation);
        }

        public UiStyle(UiStyle uiStyle)
        {
            _textColor = uiStyle.TextColor;
            _borderColor = uiStyle.BorderColor;
            _orientation = uiStyle.Orientation;
        }

        public int TextColor
        {
            get
            {
                return _textColor;
            }

            set
            {
                _textColor = value;
            }
        }

        public int BorderColor
        {
            get
            {
                return _borderColor;
            }

            set
            {
                _borderColor = value;
            }
        }

        public int Orientation
        {
            get
            {
                return _orientation;
            }

            set
            {
                _orientation = value;
            }
        }


        public void SetOrientation(string orientation)
        {

            if (orientation != null)
            {
                if (orientation.Equals("vertical"))
                    _orientation = VerticalStyle;
                else if (orientation.Equals("horizontal"))
                    _orientation = HorizontalStyle;
                else
                    _orientation = HorizontalVerticalStyle;
            }
        }

    }
}
