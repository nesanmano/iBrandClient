using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    [PublicAPI]
    public class ScreenDefinitionLargeText : IJsonConfig, IExternalFileConfig, IChangeLanguage, IOnLoad
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public string DefaultConfigFileName => "screen_def_large_text_config.json";
        public string ConfigFileName { get; set; }

        private TextBox _largeTextBox;

        /// <summary>
        /// Zone où afficher le texte.
        /// </summary>
        [JsonRequired]
        public Rectangle? TextZone { get; set; }

        /// Nom de fichier (sur chemin relatif au questionnaire) pour chaque code de langue, réflètant le contenu
        /// à afficher.
        [JsonRequired]
        public Dictionary<string, string> LargeTextFileDictionary { get; set; }
        
        public bool OnLoad(AbstractScreen screen)
        {
            if (LargeTextFileDictionary?.Count == 0)
            {
                // Pas de texte configuré.
                return false;
            }

            if (TextZone != null)
            {
                _largeTextBox = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    WordWrap = true,
                    Location = TextZone.Value.Location,
                    Size = TextZone.Value.Size,
                    Enabled = true,
                    Visible = true,
                    ReadOnly = true,
                    TextAlign = HorizontalAlignment.Left
                };

                screen.Controls.Add(_largeTextBox);
                _largeTextBox.BringToFront();
            }
            else
            {
                return false;
            }

            return true;
        }

        public void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            if (_largeTextBox == null) return;
            
            var fileName = LargeTextFileDictionary?.GetDefaultValue(languageCode);

            // Ne devrait pas arriver, sauf si on vide le dictionnaire apres initialisation. Ou si la valeur est invalide.
            if (string.IsNullOrWhiteSpace(fileName))
            {
                log.Warn("No file name deteted for language code " + fileName);
                return;
            }

            var completeFilePath = Path.Combine(screen.GetTerminalDirectory(), fileName);
            if (!File.Exists(completeFilePath))
            {
                log.Warn($"No file for for text ({languageCode}): {completeFilePath}");
                return;
            }

            screen.SafeCall(() => {
                _largeTextBox.Text = File.ReadAllText(completeFilePath);
            });
        }

        
    }
}