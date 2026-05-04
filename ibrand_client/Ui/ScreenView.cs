using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Util;
#pragma warning disable 618

namespace NomadLogic.iBrandDesktop.Ui
{
    public class ScreenView : IScreenElement
    {
        private readonly Hashtable _textTable;
        private readonly Hashtable _imageTable;

        private string _activeLanguage;

        private readonly Font _font = new Font("Arial", 10F, FontStyle.Regular);

        private Panel _panel;

        private Label _textLabel;
        
        public ScreenView()
        {
            _textTable = new Hashtable();
            _imageTable = new Hashtable();
        }

        public void SetText(string language, string text)
        {
            _textTable.Add(language, text);
        }

        public void SetImage(string language, string imageSrc)
        {
            _imageTable.Add(language, imageSrc);
        }

        public string Language
        {
            set => _activeLanguage = value;
        }

        public string GetText()
        {
            if (_activeLanguage == null)
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                _activeLanguage = configuration.PreferedLanguage;
            }

            var text = (string)_textTable[_activeLanguage];

            if (text == null)
            {
                //Get the first supported language which we have text for.
                foreach (var lg in Constants.SUPPORTED_LANGUAGES)
                {
                    text = (string)_textTable[lg];

                    if (text != null)
                        return text;
                }

                return "";
            }

            return text;
        }

        public string GetImageSrc()
        {
            if (_activeLanguage == null)
            {
                var configuration = ServicesContainer.GetService<NomadConfiguration>();
                _activeLanguage = configuration.PreferedLanguage;
            }

            var imageSrc = (string)_imageTable[_activeLanguage];

            if (imageSrc != null) return imageSrc;
            //Get the first supported language which we have text for.
            foreach (var lg in Constants.SUPPORTED_LANGUAGES)
            {
                imageSrc = (string)_imageTable[lg];

                if (imageSrc != null)
                    return imageSrc;
            }

            return "";
        }

        public int GetHeight(int availableWidth, Graphics g)
        {
            var textLabelWidth = availableWidth;

            if (textLabelWidth > Constants.MAX_WIDTH)
                textLabelWidth = Constants.MAX_WIDTH;

            var nbrOfLines = 0;
            var text = GetText();
            if (text.Length > 0)
            {
                var textWithNoLineBreak = text.Replace("[_br_]", "");
                nbrOfLines = StringUtil.GetNumberOfLines(g, _font, textLabelWidth, textWithNoLineBreak);
                nbrOfLines += StringUtil.CountStringOccurrences(text, "[_br_]");
            }

            var imageSrc = GetImageSrc();

            var imageHeight = 0;

            if (imageSrc.Length > 0)
            {
                if (File.Exists(imageSrc))
                {
                    var stream = new FileStream(imageSrc, FileMode.Open);
                    var bitmap = new Bitmap(stream);
                    stream.Close();

                    imageHeight = bitmap.Height;
                }
            }

            var height = nbrOfLines * ((int)(_font.GetHeight() * 1.1f)) + imageHeight; //Constants.LABEL_HEIGHT;

            return height + Constants.QUESTION_PAD;
        }

        public int AddToUserControl(UserControl screenPanel, int availableWidth, int y, Graphics g)
        {
            _panel = new Panel {Location = new Point(Constants.QUESTION_MARGIN, y)};

            screenPanel.Controls.Add(_panel);

            var textLabelHeight = 0;

            var text = GetText();

            if (text.Length > 0)
            {
                _textLabel = new Label
                {
                    Font = _font,
                    Text = text.Replace("[_br_]", Environment.NewLine),
                    Location = new Point(Constants.QUESTION_TEXT_MARGIN, 2)
                };

                var textLabelWidth = availableWidth;

                if (textLabelWidth > Constants.MAX_WIDTH)
                    textLabelWidth = Constants.MAX_WIDTH;

                var textWithNoLineBreak = text.Replace("[_br_]", "");

                var nbrOfLines = StringUtil.GetNumberOfLines(g, _textLabel.Font, textLabelWidth, textWithNoLineBreak);
                nbrOfLines += StringUtil.CountStringOccurrences(text, "[_br_]");

                _textLabel.Size = new Size(textLabelWidth, (int)(_font.GetHeight() * 1.1f * nbrOfLines * (g.DpiY / 96)));

                _panel.Controls.Add(_textLabel);

                textLabelHeight = _textLabel.Size.Height;
            }

            var imageSrc = GetImageSrc();

            var imageHeight = 0;

            if (imageSrc.Length > 0)
            {
                if (File.Exists(imageSrc))
                {
                    var stream = new FileStream(imageSrc, FileMode.Open);
                    var bitmap = new Bitmap(stream);
                    stream.Close();

                    var pictureBox = new PictureBox
                    {
                        Image = bitmap,
                        Height = bitmap.Height,
                        Width = availableWidth,
                        SizeMode = PictureBoxSizeMode.CenterImage
                    };


                    _panel.Controls.Add(pictureBox);

                    imageHeight = bitmap.Height;
                }
            }

            _panel.Size = new Size(availableWidth - (2 * Constants.QUESTION_MARGIN), textLabelHeight + imageHeight);

            return textLabelHeight + imageHeight + Constants.QUESTION_PAD;
        }

        public void Dispose()
        {
            _font?.Dispose();
            _panel?.Dispose();
            _textLabel?.Dispose();
        }
    }
}
