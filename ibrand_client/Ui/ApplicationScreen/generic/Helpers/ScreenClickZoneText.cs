using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Représente une zone clicquable basée sur une présentation d'une chaîne de caractère.
    /// </summary>
    [PublicAPI]
    public class ScreenClickZoneText : ScreenClickZone, IHasI18nTextDictionary
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private Font _font;

        private const TextFormatFlags FLAGS = TextFormatFlags.HorizontalCenter |
                                              TextFormatFlags.VerticalCenter |
                                              TextFormatFlags.GlyphOverhangPadding;
        
        [JsonRequired]
        public IDictionary<string, string> TextDictionary { get; set; }

        public FontDefinitionInfo FontDefinition { get; set; }
        
        public override bool OnLoad(AbstractScreen screen)
        {
            CurrentLanguage = screen.ActiveLanguage;

            if (FontDefinition == null)
            {
                var provider = ClassUtil.GetFirstValidPropertyValueFromInstance<IHasDefaultPropertyValue<FontDefinitionInfo>>(screen);
                if (provider == null || !provider.GetDefaultPropertyValue(out var def))
                {
                    // Ne possède pas de valeur de fonte, et n'est pas capable d'obtenir un défaut.
                    return false;
                }
                FontDefinition = def;
            }

            _font = StaticCustomFontRepository.CreateCustomFont(FontDefinition);

            return true;
        }

        protected override void DoPaint(Graphics g)
        {
            var text = GetCurrentText();
            if (IsVisible && text != null)
            {
                TextRenderer.DrawText(g, text, _font, Zone, FontDefinition.Color, Color.Transparent, FLAGS);
            }
        }
        
        public virtual string GetCurrentText()
        {
            var str = TextDictionary?.GetDefaultValue(CurrentLanguage);
            if (log.IsWarnEnabled && str == null)
            {
                log.Warn("No text found for text click zone. Zone={2}, Lang={0}, Dic={1}", CurrentLanguage, StringUtil.FormatEnumerable(TextDictionary?.GetEnumerator()), Zone);
            }
            return str;
        }
    }
}