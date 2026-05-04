using System;
using System.ComponentModel;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom._360
{
    public partial class SweepstakesWinner : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private SoundPlayer _player;

        public SweepstakesWinner()
        {
            InitializeComponent();

            Load += (s, e) => OnLoad();
        }

        private void OnLoad()
        {
            // Clavier
            var keyboard = new NumbersKeyboardControl
            {
                KeyPadding = 5, 
                CustomLabelInfo =
                {
                    Font = StaticCustomFontRepository.CreateCustomFont(new FontDefinitionInfo
                    {
                        Size = 15
                    })
                }
            };

            keyboard.SetLocation(27, 347);
            keyboard.Size = new Size(337, 191);

            keyboard.Parent = this;
            keyboard.BringToFront();
            keyboard.Show();

            keyboard.Init();

            _pboxGifLeft.ImageLocation = LeftImagePath;
            _pboxGifRight.ImageLocation = RightImagePath;
            _pboxMain.ImageLocation = MainImageLocation;

            _txtCode.Text = "";
            _lblMainText.Text = UserMessage;
            _lblRfid.Text = @"Carte ID : " + Rfid;

            if (SoundLocation != null)
            {
                try
                {
                    _player = new SoundPlayer();
                    _player.LoadCompleted += SoundLoadCompleted;

                    _player.SoundLocation = SoundLocation;

                    // Load the .wav file.
                    _player.LoadAsync();
                }
                catch (System.Exception e)
                {
                    log.Error(e);
                }
            }
        }

        private void SoundLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            log.Trace("Fanfare jouée!");
            _player.Play();
        }

        public string Rfid { get; set; }

        public string UserMessage { get; set; }

        public string MainImageLocation { get; set; }

        public string SoundLocation { get; set; }

        public string RightImagePath { get; set; }

        public string LeftImagePath { get; set; }

        public string Code { get; set; }

        private void _btnValidez_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Code) && !_txtCode.Text.Equals(Code)) return;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
