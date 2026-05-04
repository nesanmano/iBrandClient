using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace NomadLogic.Common.Ui.Keyboard
{
    /// <summary>
    /// Forme qui contient une séquence de clés pour former un clavier répondant aux événement souris. Utiliser pour remplacer
    /// le clavier "touch" par défaut du système, afin de l'inclure directement dans nos formes UI.
    /// Note: La position en X peut être négative. Dans un tel cas de figure, lors du calcul de la forme, on va centrer la forme via son parent.
    /// </summary>
    [PublicAPI]
    public partial class KeyboardControl : UserControl
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Lancée quand la touche "Enter" est cliquée.
        /// </summary>
        public event EventHandler OnEnterClicked;

        private bool _isCapital;
        private bool _isCapitalLock;
        private bool _isAccents;

        /// Référence directe sur le "Shift" gauche.
        protected CustomPaintLabel _lShiftBtn;

        /// Taille par défaut de chaque clé.
        protected Size _default_key_size ;

        protected Point _initialPoint;

        /// Nombre de clés par rangée, pour calculer l'emplacement. Les classes héritées peuvent modifier cette valeur dans leur constructeur.
        protected int _number_keys_in_row = 12;
        /// Tableau des clés utilisées pour initialiser le clavier. Peut être modifié par les classe héritées.
        protected KeyboardKey[] _keys;
        /// Nombre de rangées de clés.
        protected int _nbr_rows;

        private bool _initDone;

        /// <summary>
        /// Constructeur principal. On assume que la position et la taille sont donné comme propriétés de contrôles.
        /// Notamment, utilisez la propriété "Size" et la méthode "SetLocation".
        /// </summary>
        public KeyboardControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructeur avec rectangle de positionnement.
        /// </summary>
        /// <param name="positionData">Rectangle de positionnement.</param>
        public KeyboardControl(Rectangle positionData) : this() {
            _initialPoint = new Point(positionData.Location.X, positionData.Location.Y);
            Size = positionData.Size;
        }

        /// <summary>
        /// Constructeur avec rectangle de positionnement.
        /// </summary>
        /// <param name="location">Point de position</param>
        /// <param name="size">Taille du contrôle.</param>
        public KeyboardControl(Point location, Size size) : this() {
            _initialPoint = new Point(location.X, location.Y);
            Size = size;
        }

        /// <summary>
        /// Constructeur avec rectangle de positionnement.
        /// </summary>
        public KeyboardControl(int x, int y, int width, int height) : this() {
            _initialPoint = new Point(x, y);
            Size = new Size(width, height);
        }

        /// <summary>
        /// Il ne faut pas jouer avec la variable de contrôle "Location" directement pour un clavier, à cause de la façon
        /// dont cette variable est calculée une fois assignée à un parent.
        /// Pour correctement calculer la position, il faut utiliser cette méthode AVANT le "OnLoad" du cycle de ce type contrôle. Typiquement
        /// juste après sa création et avant son premier "Show".
        /// </summary>
        /// <param name="p">Le point de "location" voulu à l'écran.</param>
        public void SetLocation(Point p)
        {
            _initialPoint = p;
        }

        /// <summary>
        /// Il ne faut pas jouer avec la variable de contrôle "Location" directement pour un clavier, à cause de la façon
        /// dont cette variable est calculée une fois assignée à un parent.
        /// Pour correctement calculer la position, il faut utiliser cette méthode AVANT le "OnLoad" du cycle de ce type contrôle. Typiquement
        /// juste après sa création et avant son premier "Show".
        /// </summary>
        /// <param name="x">Le point de "location" voulu à l'écran, en X.</param>
        /// <param name="y">Le point de "location" voulu à l'écran, en Y.</param>
        public void SetLocation(int x, int y)
        {
            SetLocation(new Point(x,y));
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            BackColor = Color.Transparent;
            Text = string.Empty;
            Init();
        }

        public void Init()
        {
            if (_initDone) return;

            InitInstanceKeyData();

            CalculateDefaultKeySize();

            try
            {
                InitKeys(CustomLabelInfo);
            }
            catch (Exception err)
            {
                log.Error(err);
            }

            _initDone = true;
        }

        /// <summary>
        /// Propriétées globales des clés de clavier.
        /// </summary>
        public CustomPaintLabelInfo CustomLabelInfo { get; set; } = new CustomPaintLabelInfo();

        /// <summary>
        /// L'espace entre chaque clé.
        /// </summary>
        public int KeyPadding { get; set; } = 7;

        /// <summary>
        /// Instancie les informations du clavier propre au type de clavier.
        /// Chaque sous-classe qui veut utiliser un layout différent que la classe originale doit
        /// surcharger cette méthode.
        /// </summary>
        protected virtual void InitInstanceKeyData() {
            _number_keys_in_row = 12;
            _nbr_rows = 4;
            _keys = KeyboardKeysFactory.KeyboardUtf8;
        }

        private void OnEnterClick()
        {
            OnEnterClicked?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Calcule la taille par défaut de chaque clé clavier.
        /// </summary>
        protected void CalculateDefaultKeySize() {
            
            var nbrColumnPadding = _number_keys_in_row + 1;
            var nbrRowPadding = _nbr_rows + 1;

            var w = (Size.Width - nbrColumnPadding * KeyPadding) / nbrColumnPadding;
            var h = (Size.Height - nbrRowPadding * KeyPadding) / nbrRowPadding;

            _default_key_size = new Size(w, h);
        }

        private const int WmMouseactivate = 0x0021, MaNoactivate = 0x0003;//, MaNoactivateandeat = 0x0004, WmSetfocus = 0x0007;

        /// <inheritdoc/>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WmMouseactivate)
            {
                m.Result = (IntPtr)MaNoactivate;
                return;
            }
            base.WndProc(ref m);
        }

        private const int WsExNoactivate = 0x08000000;

        /// <inheritdoc/>
        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;
                createParams.ExStyle = WsExNoactivate;
                return createParams;
            }
        }

        /// <summary>
        /// Initialize les clés du clavier.
        /// </summary>
        /// <param name="customLabel">L'information des visuels des boutons de ce clavier.</param>
        protected void InitKeys(CustomPaintLabelInfo customLabel)
        {
            var x = KeyPadding;
            var y = KeyPadding;
            var lastSize = new Size(0, 0);
            var lastPoint = new Point(0, 0);
            
            var nbrButtons = 0;
            
            foreach (var keyboardButton in _keys)
            {
                nbrButtons += 1 * keyboardButton.Width;
                lastSize = new Size(_default_key_size.Width * keyboardButton.Width + ((keyboardButton.Width - 1) * KeyPadding), _default_key_size.Height);
                lastPoint = new Point(x, y);

                if (nbrButtons == _number_keys_in_row)
                {
                    x = KeyPadding;
                    y = y + lastSize.Height + KeyPadding;
                    nbrButtons = 0;
                }
                else
                {
                    x = x + lastSize.Width + KeyPadding;
                }

                if (keyboardButton is KeyboardKeyNone) continue;
                var button = customLabel.CreateInstance();

                button.BorderStyle = BorderStyle.None;
                button.Location = lastPoint;
                button.Name = keyboardButton.Key.ToString();
                button.UseMnemonic = false;
                button.Size = lastSize;
                button.Text = keyboardButton.GetVisual(false, false, false);


                button.MouseClick += OnButtonClick;
                button.MouseDoubleClick += OnButtonDoubleClick;

                Controls.Add(button);

                if (keyboardButton.Key == Key.LEFT_SHIFT)
                    _lShiftBtn = button;
            }
            
            var oldSize = ClientSize;

            // Calcule le point en bas à droite du dernier bouton, plus padding. Cela ajuste la taille du contrôle pour envelopper exactement les boutons à afficher.
            ClientSize = new Size(lastPoint.X + lastSize.Width + KeyPadding, lastPoint.Y + lastSize.Height + KeyPadding);

            var diffX = (oldSize.Width - ClientSize.Width) / 2;
            var diffY = (oldSize.Height - ClientSize.Height) / 2;

            // On ajute la Location avec la moitié de la différent de taille.
            var finalX = _initialPoint.X < 0 ? (Parent.Size.Width - ClientSize.Width) / 2 : _initialPoint.X + diffX;
            var finalY = _initialPoint.Y < 0 ? (Parent.Size.Width - ClientSize.Width) / 2 : _initialPoint.Y + diffY;
            Location = new Point(finalX, finalY);

            if (log.IsTraceEnabled) {
                log.Trace("Keyboard resized and relocated from ({0}, {1}, {2}, {3}) to ({4}, {5}, {6}, {7})",
                    _initialPoint.X, _initialPoint.Y, oldSize.Width, oldSize.Height,
                    Location.X, Location.Y, ClientSize.Width, ClientSize.Height);
            }
        }
        
        private void SetKeysTexts()
        {
            foreach (Component comp in Controls)
            {
                if (!(comp is CustomPaintLabel button)) continue;
                var key = FromStandardValue(button.Name);
                button.Text = key.GetVisual(_isCapital, _isCapitalLock, _isAccents);
            }
        }

        private void SetNotCapital()
        {
            _isCapital = false;
            _lShiftBtn.IsHeld = false;
            _lShiftBtn.Invalidate();
        }

        /// <summary>
        /// Identifie une clé selon sa valeur par défaut (non capitalisée, non-accentuée).
        /// </summary>
        /// <param name="standardValue">Valeur standard cherchée.</param>
        /// <returns>La clé de clavier, si existante.</returns>
        public KeyboardKey FromStandardValue(string standardValue)
        {
            return _keys.FirstOrDefault(t => t.Key.ToString().Equals(standardValue));
        }

        protected void OnButtonDoubleClick(object sender, MouseEventArgs e) {
            if (sender is CustomPaintLabel btn) KeyClick(btn);
        }

        private void KeyClick(CustomPaintLabel btn)
        {
            var kb = FromStandardValue(btn.Name);
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (kb.Key)
            {
                case Key.NONE:
                    // Ne fait strictement rien. No-Op
                    break;
                case Key.CAPS_LOCK:
                    _isCapitalLock = !_isCapitalLock;
                    btn.IsHeld = _isCapitalLock;
                    break;
                case Key.LEFT_SHIFT:
                    _isCapital = !_isCapital;
                    btn.IsHeld = _isCapital;
                    break;
                case Key.ACCENTS_LETTERS:
                    _isAccents = !_isAccents;
                    btn.IsHeld = _isAccents;
                    break;
                case Key.ARROW_DOWN:
                case Key.ARROW_LEFT:
                case Key.ARROW_RIGHT:
                case Key.ARROW_UP:
                case Key.BACKSPACE:
                    SendKeys.Send(kb.GetValue(false, false, false));
                    // On ne désactive pas la capitalisation.
                    break;
                case Key.ENTER:
                    SendKeys.Send(kb.GetValue(false, false, false));
                    OnEnterClick();
                    break;
                default:
                    SendKeys.Send(kb.GetValue(_isCapital, _isCapitalLock, _isAccents));

                    if (_isCapital)
                    {
                        SetNotCapital();
                    }
                    break;
            }

            SetKeysTexts();
        }

        protected void OnButtonClick(object sender, MouseEventArgs e) {
            if (sender is CustomPaintLabel btn) KeyClick(btn);
        }

    }
}