using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.PrintStation.Ui.generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;

namespace NomadLogic.PrintStation.Ui.custom.Brewers
{
    class BrewersQuestionnaireScreen : AbstractPrintStationScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly Rectangle _nextArea;

        private ImageListTransformOptions _imageListTransform;

        private TextBox _nameTextbox;
        private TextBox _homeTownTextbox;
        //private TextBox _ageTextbox;
        private ComboBox _positionTextbox;
        private ComboBox _batsTextbox;
        private ComboBox _throwsTextbox;

        
        public Rectangle? CancelZone { get; set; }

        /// Cette variable est une sémaphone multi-thread pour indiquer que la classe travaille en arrière plan (soit attends une photo, soit est en train
        /// de transformer quelque chose). Si mis à vrai, ne devrait pas réagir sur certains événements, comme le UI.
        protected volatile bool _backgroundWorkActive;

        public BrewersQuestionnaireScreen(string bkgEn, string bkgFr, PrintStationScreenConfig sc)
            : base(bkgEn, bkgFr, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            _nextArea = new Rectangle(759, 894, 164, 78);
        }

        public override void Recycle()
        {
            base.Recycle();

            _nameTextbox.Text = "";
            _homeTownTextbox.Text = "";
            //_ageTextbox.Text = "";
            _positionTextbox.SelectedIndex = 7; // Pitcher
            _batsTextbox.SelectedIndex = 1; // Right
            _throwsTextbox.SelectedIndex = 1; // Right

        }

        private void AddKeyboard()
        {
            var kb = new KeyboardControl(59, 419, 1563, 459);
            AddKeyboardToControls(kb);
        }


        /// <summary>
        /// Contient l'ensemble des options de transformations avec des fichiers d'image.
        /// Si cette variable n'est pas explicitement donnée, on tente de lire un fichier dans le répertoire du projet, qu'il existe ou non, et on met à jour
        /// la configuration selon le contenu de ce fichier.
        /// </summary>
        public ImageListTransformOptions ImageListTranformOptions
        {
            get
            {
                if (_imageListTransform != null) return _imageListTransform;

                var transform = new ImageListTransformOptions();
                JsonUtil.UpdateFromJson(
                    new FileInfo(GetTerminalDirectory() + transform.DefaultConfigFileName),
                    transform);
                return transform;
            }
            set => _imageListTransform = value;
        }
        

        private void AddTextBoxs()
        {
            var allSize = new Size(582,71);
            var cols = new[] { 230, 1040 };
            var rows = new[] { 203, 274, 354 };
            var font = new Font(FontFamily.GenericSansSerif, 20);
            var index = 0;
            var posSelection = new object[]
            {
                "FIRST BASEMAN", "SECOND BASEMAN", "SHORTSTOP", "THIRD BASEMAN", "LEFT FIELDER", "CENTER FIELDER", "RIGHT FIELDER", "PITCHER", "CATCHER"
            };
            var handSelection = new object[]
            {
                "LEFT", "RIGHT"
            };

            this.InitTextBox( out _nameTextbox,      new Rectangle(new Point(cols[0], rows[0]), allSize), font, ++index);
            this.InitTextBox( out _homeTownTextbox,  new Rectangle(new Point(cols[0], rows[1]), allSize), font, ++index);
            //InitTextBox( out _ageTextbox,       new Rectangle(new Point(cols[0], rows[2]), allSize), font, ++index);
            this.InitComboBox(out _positionTextbox,  new Rectangle(new Point(cols[1], rows[0]), allSize), font, ++index, posSelection);
            this.InitComboBox(out _batsTextbox,      new Rectangle(new Point(cols[1], rows[1]), allSize), font, ++index, handSelection);
            this.InitComboBox(out _throwsTextbox,    new Rectangle(new Point(cols[1], rows[2]), allSize), font, ++index, handSelection);

            _positionTextbox.SelectedIndex = 7; // Pitcher
            _batsTextbox.SelectedIndex = 1; // Right
            _throwsTextbox.SelectedIndex = 1; // Right
        }


        protected override bool OnMouseDownHandler(int x, int y)
        {
           
            if (InArea(x, y, _nextArea))
            {
                if (SetAnswersToQuestionnaire())
                {
                    Next(1);
                }

                return false;
            }
            
            if (InArea(x, y, CancelZone))
            {
                Close();
                return false;
            }
            
            return false;
        }
        
        private bool SetAnswersToQuestionnaire()
        {
            MessageDialog mBox;

            // Validation nom
            if (!string.IsNullOrWhiteSpace(_nameTextbox.Text))
            {
                //SaveAnswerToQuestion(5, 1, _nameTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Name is mandatory.");

                return false;
            }

            // Validation hometown
            if (!string.IsNullOrWhiteSpace(_homeTownTextbox.Text))
            {
                //SaveAnswerToQuestion(6, 1, _homeTownTextbox.Text.Trim());
            }
            else
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Home town is mandatory.");

                return false;
            }

            // Validation age
            /*
            if (int.TryParse(_ageTextbox.Text, out var age))
            {
                SaveAnswerToQuestion(7, 1, age.ToString());
            }
            else
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Age is mandatory (and must be a valid number).");

                return false;
            }
            */

            // Validation position
            if (_positionTextbox.SelectedItem != null)
            {
//                var answer = new Answer(-1,8,1, _positionTextbox.SelectedIndex + 1);
//                SaveAnswerToQuestion(8, 1, answer);
            }
            else
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Please select a position.");

                return false;
            }

            // Validation bats
            if (_batsTextbox.SelectedItem != null)
            {
//                var answer = new Answer(-1,9, 1, _batsTextbox.SelectedIndex + 1);
//
//                SaveAnswerToQuestion(9, 1, answer);
            }
            else
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Please select a batting side");

                return false;
            }


            // Validation bats
            if (_throwsTextbox.SelectedItem != null)
            {
//                var answer = new Answer(-1, 10, 1, _throwsTextbox.SelectedIndex + 1);
//
//                SaveAnswerToQuestion(10, 1, answer);
            }
            else
            {
                mBox = new MessageDialog();

                mBox.ShowMessage("Please select throwing hand.");

                return false;
            }


            return true;
        }

        public override void SetActivated()
        {
            base.SetActivated();

            var city = GetTextAnswerFromQuestion(6) ?? "";
            _homeTownTextbox.Text = city;

            var name = GetTextAnswerFromQuestion(5) ?? "";
            _nameTextbox.Text = name;
            _nameTextbox.Focus();
        }

    }
}
