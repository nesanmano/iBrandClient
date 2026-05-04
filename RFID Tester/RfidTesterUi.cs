using System;
using System.Threading;
using System.Windows.Forms;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Emisys;
using NomadLogic.Common.Util.RfidReader;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.RfidTest
{
    public partial class RfidTesterUi : Form
    {
        private IRfidReader _currentReader;

        public RfidTesterUi()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _lblResult.Text = "";
            _txtResult.Enabled = true;

            _cmbReader.SelectedIndexChanged += LoadReader;
            _cmbReader.SelectedIndex = 0;

            _txtInput.TextAlign = HorizontalAlignment.Center;
            _txtInput.TextChanged += (sender, args) => OnTextChangedScanTrigger();
            _txtInput.KeyDown += SuppressEnterKey;
            _txtInput.Select();
        }

        private void SuppressEnterKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            //enter key is down
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void LoadReader(object sender, EventArgs e)
        {
            _currentReader?.Dispose();
            _txtResult.Text = "";
            _lblResult.Text = "";
            _txtInput.Select();

            switch (_cmbReader.SelectedIndex)
            {
                case 0:
                    _currentReader = new Simple_RfidReader
                    {
                        AllowDirectHexData = true,
                        KeepSameByteOrder = true,
                        TimeBetweenRead = TimeSpan.FromSeconds(1),
                        TimeBetweenSameCardRead = TimeSpan.FromSeconds(1)
                    };
                    break;
                case 1:
                    MessageBox.Show(@"Currently not implemented", @"iBrand", MessageBoxButtons.OK);
                    break;
            }
        }

        private void btnForceTest_Click(object sender, EventArgs e)
        {
            CheckCodeEvent();
        }

        private void OnTextChangedScanTrigger()
        {
            if (_currentReader != null && _currentReader.ScanTriggerOnCharacterNumber > 0 && _txtInput.Text.Length < _currentReader.ScanTriggerOnCharacterNumber) return;
            this.SafeCall(CheckCodeEvent);
        }

        private async void CheckCodeEvent()
        {
            _txtResult.Text = "";
            
            if (_currentReader == null)
            {
                _lblResult.Text = @"No reader.";
                return;
            }

            var cardId = _currentReader.ExecuteSequenceAnalisys(_txtInput.Text);
            _lblReadValue.Text = _txtInput.Text;
            _lblHexValue.Text = cardId.RawCode;

            var req = new WebRequestEmisysPayWall
            {
                DoNotTestEndPointInit = true,
                LoginInfo = new EmisysApiLoginInfo
                {
                    Apikey = "Nu/9qmq6e38NFpB..vq6",
                    RootApiUrl = "https://live.byemisys.com/api",
                    UserEmail = "api@nomadlogic.com",
                    Version = "1"
                }
            };

            var result = await req.FinalizeCardFullAnswer(cardId.FinalCode, CancellationToken.None);

            if (result?.InstancedBody == null)
            {
                _lblResult.Text = @"Request failed";
                _txtResult.Text = "";
            }
            else if (result.InstancedBody.Result == 2)
            {
                if (result.InstancedBody.Status.Contains("initialized"))
                {
                    _lblResult.Text = @"Success - Not Initialized";
                    
                }
                else if (result.InstancedBody.Status.Equals("card_not_found_in_db"))
                {
                    _lblResult.Text = @"Failure - Not in Database";
                }
                else
                {
                    _lblResult.Text = @"Unknown status code. Check full data below";
                }

                _txtResult.Text = ClassUtil.ToStringPublicProperties(result.InstancedBody, 1, "\r\n", "");
                
            }
            else
            {
                _lblResult.Text = @"Success - In Database";
                _txtResult.Text = ClassUtil.ToStringPublicProperties(result.InstancedBody, 1, "\r\n", "");
            }

            _txtInput.Text = "";
            _txtInput.Select();
        }

        protected void ResetScanArea()
        {
            this.SafeCall(() =>
            {
                if (_txtInput == null) return;
                _txtInput.Text = "";
                _txtInput.Focus();
            });
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }
    }
}
