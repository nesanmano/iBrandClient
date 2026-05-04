using System.Drawing;
using JetBrains.Annotations;

namespace NomadLogic.Common.Ui.Keyboard
{
    /// <summary>
    /// Variation du clavier pour ne contenir que des touches numériques.
    /// </summary>
    [PublicAPI]
    public class NumbersKeyboardControl : KeyboardControl {

        /// <summary>
        /// Constructeur avec rectangle de positionnement.
        /// </summary>
        /// <param name="positionData">Rectangle de positionnement.</param>
        public NumbersKeyboardControl(Rectangle positionData) : base(positionData) {}

        /// <summary>
        /// Constructeur avec rectangle de positionnement.
        /// </summary>
        /// <param name="location">Point de position</param>
        /// <param name="size">Taille du contrôle.</param>
        public NumbersKeyboardControl(Point location, Size size) : base(location, size) {}

        /// <summary>
        /// Constructeur avec rectangle de positionnement.
        /// </summary>
        public NumbersKeyboardControl(int x, int y, int width, int height) : base(x, y, width, height) {}

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public NumbersKeyboardControl() {}

        /// <inheritdoc />
        protected override void InitInstanceKeyData() {
            _number_keys_in_row = 3;
            _nbr_rows = 3;
            _keys = KeyboardKeysFactory.KeyboardNumbersButtons;
        }
    }
}
