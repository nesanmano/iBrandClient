using System.Drawing;

namespace NomadLogic.Common.Ui.Keyboard
{
    public class KeyboardEmail: KeyboardControl {

        /// <inheritdoc />
        public KeyboardEmail(Rectangle positionData) : base(positionData) {}

        /// <inheritdoc />
        public KeyboardEmail(Point location, Size size) : base(location, size) {}

        /// <inheritdoc />
        public KeyboardEmail(int x, int y, int width, int height) : base(x, y, width, height) {}

        /// <inheritdoc />
        public KeyboardEmail() {}

        /// <inheritdoc />
        protected override void InitInstanceKeyData() {
            _number_keys_in_row = 16;
            _nbr_rows = 4;
            _keys = KeyboardKeysFactory.KeyboardUtf8Email;
        }
    }
}