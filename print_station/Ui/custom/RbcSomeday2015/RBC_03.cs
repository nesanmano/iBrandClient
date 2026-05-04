using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Ui.generic;

namespace NomadLogic.PrintStation.Ui.custom.RbcSomeday2015
{
    public class Rbc03 : AbstractPrintStationScreen
    {
        readonly Rectangle _nextArea;
        readonly Rectangle _previousArea;
        private TextBox _textBox;
        private readonly Brush _whiteBrush = Brushes.White;
        private readonly Brush _blackBrush = Brushes.Black;
        private readonly PrivateFontCollection _customFont = new PrivateFontCollection();
        private string _lastTransformedName;
        private string _lastTransformedText;

        public Rbc03(string backgroundImgEnSrc, PrintStationScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgEnSrc, sc)
        {
            AddTextBoxs();
            AddKeyboard();

            //PictureBox.MouseClick += MouseDownHandler;

            _nextArea = new Rectangle(new Point(650, 350), new Size(175, 60));
            _previousArea = new Rectangle(new Point(455, 350), new Size(175, 60));

            _customFont.AddFontFile(GetTerminalDirectory() + "american-typewriter.ttf");
        }

        public override void Recycle()
        {
            base.Recycle();
            _textBox.Text = "";
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls(100, 425, 1160, 310);
        }

        private bool SetAnswersToQuestionnaire()
        {
            if (_textBox.Text.Length > 0)
            {
                //SaveAnswerToQuestion(5, 1, _textBox.Text);
                return true;
            }
            var mBox = new MessageDialog();
            mBox.ShowMessage("Please answer all questions.");
            return false;
        }

        private void AddTextBoxs()
        {
            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Multiline = true,
                Location = new Point(585, 230),
                Size = new Size(550, 75),
                TabIndex = 1,
                Font = new Font(FontFamily.GenericSansSerif, 24)
            };
            _textBox.TextChanged += OnTextChanged;

            Controls.Add(_textBox);
            _textBox.BringToFront();
        }

        //protected override void MouseDownHandler(object sender, MouseEventArgs e)
        //{
        //    base.MouseDownHandler(sender, e);

        //    if (InArea(e.X, e.Y, _nextArea))
        //    {
        //        if (!SetAnswersToQuestionnaire()) return;

        //        if (DoTransformations())
        //        {
        //            Next();
        //        }
        //    }
        //    else if (InArea(e.X, e.Y, _previousArea))
        //    {
        //        Back();
        //    }
        //}
        
        public override void SetActivated()
        {
            base.SetActivated();
            _textBox.Focus();
        }

        private bool DoTransformations()
        {
            var isSuccessful = false;

            // 0- Ne rien faire si l'image transformée existe déjà.
            var transformedFile = GetTextAnswerFromQuestion(10, 1);

            var doTransform = !File.Exists(transformedFile);

            var currentName = GetTextAnswerFromQuestion(3, 1);

            if (!currentName.Equals(_lastTransformedName))
            {
                doTransform = true;
            }

            var textPlay = GetTextAnswerFromQuestion(5, 1);

            if (!textPlay.Equals(_lastTransformedText))
            {
                doTransform = true;
            }

            if (doTransform)
            {
                var pictureFile = GetSelectedPicture().Src;
                
                //transformedFile = GetCurrentQuestionnaireTransformedPictureDir() + GenerateUniqueId.GetId() + "_" + StringUtil.GetFileName(pictureFile);
                
                // 1- Si ce n'est déjà fait, sauvegarde l'image initiale.
                var basePicture = GetTextAnswerFromQuestion(1, 1);
                if (!File.Exists(basePicture))
                {
                    //basePicture = GetCurrentQuestionnairePictureDir() + GenerateUniqueId.GetId() + "_" + StringUtil.GetFileName(pictureFile);
                    File.Copy(pictureFile, basePicture);

                    if (File.Exists(basePicture))
                    {
                        //SaveAnswerToQuestion(1, 1, basePicture);
                    }
                    else
                    {
                        throw new Exception("Could not save post picture to questionnaire picture directory.");
                    }
                }

                // 2- Créer le canevas (fond blanc)
                /* Taille finale: 1800x1200, Image à transformer => Resize à 1060x1060 */

                using (var bmpFinal = new Bitmap(1800, 1200, PixelFormat.Format32bppArgb))
                {
                    bmpFinal.SetResolution(300, 300);

                    using (var g = Graphics.FromImage(bmpFinal))
                    {
                        g.FillRectangle(_whiteBrush, 0, 0, bmpFinal.Width, bmpFinal.Height);

                        // 3- Appliquer l'image initiale
                        using (var imageInit = FileUtil.LoadBitmap(basePicture))
                        {
                            imageInit.SetResolution(300, 300);
                            using (var resizedBase = imageInit.Resize(1060, 1060))
                            {
                                resizedBase.SetResolution(300, 300);
                                g.DrawImageUnscaled(resizedBase, 53, 53);
                            }
                        }

                        // 4- Appliquer le overlay
                        string overlayFile = null;//DirectoryUtil.GetLocalIbrandQuestionnaireDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId) + "overlay.png";
                        using (var overlay = FileUtil.LoadBitmap(overlayFile))
                        {
                            overlay.SetResolution(300, 300);
                            g.DrawImageUnscaled(overlay, 0, 0);
                        }

                        // 5a- Créer le texte de la pièce
                        var font = new Font(_customFont.Families[0], 10, FontStyle.Regular);
                        
                        g.RotateTransform(-10);

                        SizeF size;
                        var lignes = BitmapExtentions.SplitStringForLineRendering(600, textPlay, g, font);

                        for (var i = 0; i < lignes.Count; i++)
                        {
                            size = g.MeasureString(lignes[i], font);
                            g.DrawString(lignes[i], font, _blackBrush, 1300 - (size.Width/2), 530 + (i * 50));
                        }

                        font = new Font(_customFont.Families[0], 14, FontStyle.Regular);
                        size = g.MeasureString(currentName, font);
                        g.DrawString(currentName, font, _blackBrush, 1300 - (size.Width/2), 780);

                        g.ResetTransform();

                        _lastTransformedName = currentName;
                        _lastTransformedText = textPlay;
                    }

                    // 6- Sauvegarder l'image transformée.
                    var tmpFile = DirectoryUtil.CreateLocalTemporaryFile(GenerateUniqueId.GetId() + ".tmp");
                    if (!bmpFinal.Save(ImageFormat.Jpeg, 95, tmpFile)) return isSuccessful;
                    if (File.Exists(transformedFile))
                    {
                        File.Delete(transformedFile);
                    }
                    File.Move(tmpFile, transformedFile);
                    //isSuccessful = SaveAnswerToQuestion(10, 1, transformedFile);
                }
            }
            else
            {
                isSuccessful = true;
            }

            return isSuccessful;
        }
    }
}
