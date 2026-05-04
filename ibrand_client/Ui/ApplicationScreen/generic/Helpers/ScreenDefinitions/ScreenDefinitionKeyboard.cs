using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    public class ScreenDefinitionKeyboard : IJsonConfig, IExternalFileConfig, IOnLoad
    {
        public string DefaultConfigFileName { get; set; } = "screen_keyboard_definition.json";

        public string ConfigFileName { get; set; }
        
        public Rectangle Position { get; set; }

        public int Padding { get; set; } = 7;

        public FontDefinitionInfo FontDefinition { get; set; }

        public Color ColorContour { get; set; } = Color.Transparent;

        public float WidthContour { get; set; } = 2.0f;

        public Color ColorMouseDown { get; set; } = Color.FromArgb(128, Color.Gray);

        public Color ColorSelected { get; set; } = Color.FromArgb(128, Color.Chocolate);

        public IList<Color> ColorGradient { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public KeyboardTypeEnum KeyboardType { get; set; } = KeyboardTypeEnum.Multilingual;

        public bool OnLoad(AbstractScreen screen)
        {
            KeyboardControl keyboard;

            // TODO Il faudrait avoir un factory pour les claviers.
            switch (KeyboardType)
            {
                case KeyboardTypeEnum.Number: keyboard = new NumbersKeyboardControl(Position); break;
                case KeyboardTypeEnum.Email: keyboard = new KeyboardEmail(Position); break;
                case KeyboardTypeEnum.Us: keyboard = new UsaKeyboardControl(Position); break;
                default: keyboard = new KeyboardControl(Position); break; // Multilingue
            }

            keyboard.KeyPadding = Padding;

            keyboard.CustomLabelInfo = new CustomPaintLabelInfo
            {
                ColorPalette = ColorGradient ?? new List<Color>(),
                ContourPen = new Pen(ColorContour, WidthContour),
                MouseDownColor = ColorMouseDown,
                SelectedColor = ColorSelected
            };

            if (FontDefinition != null)
            {
                keyboard.CustomLabelInfo.Font = StaticCustomFontRepository.CreateCustomFont(FontDefinition);
                keyboard.CustomLabelInfo.TextBrush = StaticCustomFontRepository.CreateCustomTextBrush(FontDefinition);
            }

            keyboard.SetLocation(Position.X, Position.Y);
            keyboard.Size = Position.Size;

            screen.AddKeyboardToControls(keyboard);

            keyboard.Init();

            return true;
        }
    }

    public enum KeyboardTypeEnum
    {
        Multilingual,
        Number,
        Email,
        Us
    }
}