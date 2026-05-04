using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.Choice;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.Groslch
{
    class QuestionnaireScreen : AbstractPrintStationScreen
    {

        private readonly Rectangle _closeArea;
        private readonly Rectangle _nextArea;
        private readonly Rectangle _optInArea1;
        private readonly Rectangle _optInArea2;
        private readonly TextBox _firstNameLastName;

        private Bitmap _checkmark;

        private bool _optInChecked1;
        private bool _optInChecked2;

        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZone { get; set; }

        public QuestionnaireScreen(string backgroundImgEnSrc, PrintStationScreenConfig sc) : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            //PictureBox.MouseClick += MouseDownHandler;

            AddKeyboard();

            _checkmark = GetNoneLanguageBitmap("checkmark.png");
            _checkmark.SetResolution(300, 300);

            _firstNameLastName = new TextBox
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(361, 36),
                Size = new Size(785, 43),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 20)
            };
            Controls.Add(_firstNameLastName);
            _firstNameLastName.BringToFront();

            _nextArea = new Rectangle(new Point(563, 197), new Size(154, 43));
            _closeArea = new Rectangle(new Point(1199, 2), new Size(80, 66));

            _optInArea1 = new Rectangle(42, 91, 34, 34);
            _optInArea2 = new Rectangle(42, 154, 34, 34);

        }


        private void AddKeyboard()
        {
            // AddKeyboardToControls((Size.Width - 415) / 2, 436, 415, 333);
            var key = new KeyboardControl(6, 259, 1268, 326)
            {
                Parent = PictureBox
            };

            key.BringToFront();
            key.Show();
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_firstNameLastName.Text.Length == 0)
            {
                var mBox = new MessageDialog();
                mBox.ShowMessage("Please enter your first and last name.");
                return false;
            }

            /* Code pour la transformation  */
            var socPicPath = GetTerminalDirectoryDataFile( GenerateUniqueId.GetId() + ".jpg",
                                 DirectoryUtil.ProjectDataDirectoriesEnum.pictures,
                                 DirectoryUtil.ProjectDataDirectoriesModEnum.socialwall);
            using (Bitmap 
                bmp = new Bitmap(600, 900, PixelFormat.Format24bppRgb), 
                selected = FileUtil.LoadBitmap(GetSelectedPicture().Src))
            {
                bmp.AddOverlay(selected, -150, -150);
                bmp.Save(ImageFormat.Jpeg, 90, socPicPath);
            }
//            SaveAnswerToQuestion(2, 1, socPicPath);
//
//            SaveAnswerToQuestion(1, 1, GetSelectedPicture().Src);
//
//            SaveAnswerToQuestion(5, 1, _firstNameLastName.Text);
//
//            SaveAnswerToQuestionCheck(6, 1, _optInChecked1);
//
//            SaveAnswerToQuestionCheck(7, 1, _optInChecked2);

            return true;
        }

        /*protected  void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (InArea(e.X, e.Y, _closeArea))
            {
                Close();
            }
            else if (InArea(e.X, e.Y, _nextArea))
            {
               if (SetAnswersToQuestionnaire()) Next();
            }
            else if (InArea(e.X, e.Y, _optInArea1))
            {
                _optInChecked1 = !_optInChecked1;
                Refresh();
            }
            else if (InArea(e.X, e.Y, _optInArea2))
            {
                _optInChecked2 = !_optInChecked2;
                Refresh();
            }
        }

        protected void PaintEvent(object sender, PaintEventArgs e)
        {
            

            if (_optInChecked1)
            {
                e.Graphics.DrawImage(_checkmark, 48, 100,
                    new Rectangle(0, 0, _checkmark.Width, _checkmark.Height), 
                    GraphicsUnit.Pixel);
            }

            if (_optInChecked2)
            {
               e.Graphics.DrawImage(_checkmark, 48, 161,
                    new Rectangle(0, 0, _checkmark.Width, _checkmark.Height),
                    GraphicsUnit.Pixel);

            }
        }*/

        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }

        public override void Recycle()
        {
            base.Recycle();

            _optInChecked1 = false;
            _optInChecked2 = false;
            _firstNameLastName.Text = "";
        }

        public override void SetActivated()
        {
            base.SetActivated();
            _firstNameLastName.Focus();
        }
    }
}
