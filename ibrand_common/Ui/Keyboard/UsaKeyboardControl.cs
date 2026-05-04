using System.Drawing;

namespace NomadLogic.Common.Ui.Keyboard
{
    /// <summary>
    /// Variation du clavier pour afficher un "look and feel" semblable au clavier US Standard.
    /// </summary>
    public class UsaKeyboardControl : KeyboardControl {

        /// <inheritdoc />
        public UsaKeyboardControl(Rectangle positionData) : base(positionData) {}

        /// <inheritdoc />
        public UsaKeyboardControl(Point location, Size size) : base(location, size) {}

        /// <inheritdoc />
        public UsaKeyboardControl(int x, int y, int width, int height) : base(x, y, width, height) {}

        /// <inheritdoc />
        public UsaKeyboardControl() {}

        /// <inheritdoc />
        protected override void InitInstanceKeyData() {
            _number_keys_in_row = 14;
            _nbr_rows = 5;
            _keys = KeyboardKeysFactory.KeyboardUtf8Us;
        }
    }
}
