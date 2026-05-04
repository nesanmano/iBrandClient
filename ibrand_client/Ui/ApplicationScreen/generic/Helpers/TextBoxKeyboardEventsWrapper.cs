using System;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Cette classe gère les événements claviers d'une boîte de texte pour réagir dans deux situations optionnelles et complémentaires:
    /// - Soit un nombre minimal de caractères à été détecté (supérieur à zéro).
    /// - Soit la touche "Enter" à été pressée (avec plus que un caractère existant).
    /// Quand un événement est détecté, le contenu de la boîte texte est vidé.
    /// Il est possible de créer ce wrapper avec aucun événements ci-haut: c'est possible car c'est un aide de configuration
    /// d'éléments écran. Dans cette situation, le wrapper sera inerte, mais au moins le code sera constant dans toutes les situation.
    /// </summary>
    [PublicAPI]
    public class TextBoxKeyboardEventsWrapper: IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private bool _enterDetected;
        private readonly TextBox _textBox;
        private readonly TextBoxTriggerDelegate _triggerAction;
        private readonly int _minPress;

        /// <summary>
        /// Type de l'Action appellée quand un événement survient.
        /// </summary>
        public delegate void TextBoxTriggerDelegate(string text);

        public TextBoxKeyboardEventsWrapper([NotNull] TextBox box, TextBoxTriggerDelegate triggerAction, bool useEnter, int? minKeyPress = null)
        {
            _textBox = box;
            _triggerAction = triggerAction;
            
            if (useEnter)
            {
                _textBox.KeyDown += OnTextBoxKeyDown;
                _textBox.KeyPress += OnTextBoxKeyPress;
            }

            if (minKeyPress != null)
            {
                _minPress = minKeyPress.Value;
                _textBox.TextChanged += OnTextBoxTextChanged;
            }
        }

        private void OnTextBoxTextChanged(object sender, EventArgs e)
        {
            if (_minPress <= 0) return;
            var code = _textBox.Text;
            var visCount = code.Trim().Length;

            log.Trace("Analyzing text length: {0}", code);
            if (visCount < _minPress) return;
            log.Debug("Automatic trigger launched by minimal number of caracter.");
            
            _triggerAction(code);
            ResetTextBox();
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return; // Rien de plus. N'empêche pas les caractères autre que return d'être processés.
            log.Trace("Enter key pressed.");
            _enterDetected = true;
        }

        private void OnTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!_enterDetected) return;
            e.Handled = true;
            _enterDetected = false;
            var code = _textBox.Text;
            if (string.IsNullOrWhiteSpace(code)) return;
            _triggerAction(code);
            ResetTextBox();
        }

        public void ResetTextBox()
        {
            _textBox.SafeCall(() =>
            {
                if (_textBox == null) return;
                _textBox.Text = "";
                _textBox.Focus();
            });
        }

        public void Dispose()
        {
            _textBox.KeyDown -= OnTextBoxKeyDown;
            _textBox.KeyPress -= OnTextBoxKeyPress;
            _textBox.TextChanged -= OnTextBoxTextChanged;
        }
    }
}