using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Drawing.Text;
using NomadLogic.iBrand.Common.Respondent;
using NomadLogic.iBrand.Common.Util;

namespace NomadLogic.iBrand.Transformation.Layer
{
    class TextLayer : AbstractLayer
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private List<QuestionChoice> _questionChoiceList;

        private string _valueFromConfig;
        private string _font;
        private int _size = -1;
        private FontStyle _style = FontStyle.Regular;
        private bool _center;
        private bool _upperCase;
        private bool _lowerCase;
        private Color _color = Color.Empty;

        public TextLayer(int x, int y)
            : base(x, y)
        {
        }

        public TextLayer(int x, int y, XmlReader reader)
            : this(x, y)
        {
            // ancienne version
            var questionNoRead = reader.GetAttribute("question_no");
            if (questionNoRead != null)
                QuestionNo = int.Parse(questionNoRead);

            var choiceNoRead = reader.GetAttribute("choice_no");
            if (choiceNoRead != null)
                ChoiceNo = int.Parse(choiceNoRead);

            // fin ancienne version

            var value = reader.GetAttribute("value");
            if (value != null)
                Value = value;

            var size = reader.GetAttribute("size");
            if (size != null)
                Size = int.Parse(size);

            var font = reader.GetAttribute("font");
            if (font != null)
                Font = font;

            var color = reader.GetAttribute("color");
            if (color != null)
                TextColor = Color.FromArgb(int.Parse(color));

            var style = reader.GetAttribute("style");
            if (style != null)
            {
                var styleValue = int.Parse(style);
                switch (styleValue)
                {
                    case 0:
                        Style = FontStyle.Regular;
                        break;
                    case 1:
                        Style = FontStyle.Bold;
                        break;
                    case 2:
                        Style = FontStyle.Italic;
                        break;
                }
            }

            var center = reader.GetAttribute("center");
            if (center == "true")
                Center = true;

            var upperCase = reader.GetAttribute("upper_case");
            if (upperCase == "true")
                UpperCase = true;

            var lowerCase = reader.GetAttribute("lower_case");
            if (lowerCase == "true")
                LowerCase = true;

            var widthFromConfig = reader.GetAttribute("width");
            if (widthFromConfig != null)
                WidthFromConfig = int.Parse(widthFromConfig);

            var justify = reader.GetAttribute("justify");
            if (justify != null)
                Justify = justify;

            var prefix = reader.GetAttribute("prefix");
            if (prefix != null)
                Prefix = prefix;

            var suffix = reader.GetAttribute("suffix");
            if (suffix != null)
                Suffix = suffix;

            if (!reader.IsEmptyElement)
            {
                _questionChoiceList = new List<QuestionChoice>();

                var layerFinished = false;

                while (!layerFinished)
                {
                    reader.Read();
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals("question"))
                            {
                                var questionNo = reader.GetAttribute("no");
                                var choiceNo = reader.GetAttribute("choice_no");
                                if (questionNo != null && choiceNo != null)
                                    AddQuestionChoice(new QuestionChoice(int.Parse(questionNo), int.Parse(choiceNo)));
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name.Equals("layer"))
                            {
                                layerFinished = true;
                            }
                            break;
                    }
                }
            }
        }

        public TextLayer(TextLayer original)
            : base(original)
        {
            if (original._questionChoiceList != null)
            {
                foreach (var qc in original._questionChoiceList)
                {
                    _questionChoiceList.Add(qc);
                }
            }

            _valueFromConfig = original._valueFromConfig;
            _font = original._font;
            _size = original._size;
            _style = original._style;
            _center = original._center;
            _upperCase = original._upperCase;
            _lowerCase = original._lowerCase;
            _color = original._color;
            WidthFromConfig = original.WidthFromConfig;
            Justify = original.Justify;

            Prefix = original.Prefix;
            Suffix = original.Suffix;
        }

        public string Value
        {
            get { return _valueFromConfig; }
            set { _valueFromConfig = value; }
        }

        public string Font
        {
            get { return _font; }
            set { _font = value; }
        }

        public bool Center
        {
            get { return _center; }
            set { _center = value; }
        }

        public bool UpperCase
        {
            get { return _upperCase; }
            set { _upperCase = value; }
        }

        public bool LowerCase
        {
            get { return _lowerCase; }
            set { _lowerCase = value; }
        }

        public void AddQuestionChoice(QuestionChoice questionChoice)
        {
            if (_questionChoiceList == null)
                _questionChoiceList = new List<QuestionChoice>();

            _questionChoiceList.Add(questionChoice);
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public FontStyle Style
        {
            get { return _style; }
            set { _style = value; }
        }

        public Color TextColor
        {
            get { return _color; }
            set { _color = value; }
        }

        public override int GetImageWidth(IRespondent respondent)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public override int GetImageHeight(IRespondent respondent)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public override void Draw(Bitmap image, IRespondent respondent)
        {
            var text = GetText(respondent);
            if (text != null && !text.Equals(""))
            {
                if (Prefix != null)
                    text = Prefix + text;
                if (Suffix != null)
                    text = text + Suffix;

                var g2D = Graphics.FromImage(image);
                var newFont = GetConfigFont();
                if (!newFont.FontFamily.Name.Contains(Font))
                {
                    log.Debug("Font " + Font + "not found, using " + newFont.FontFamily.Name + " instead.");
                    var fontsCollection = new InstalledFontCollection();
                    foreach (var fontFamiliy in fontsCollection.Families)
                    {
                        log.Debug(fontFamiliy.Name);
                    }
                }
                if (WidthFromConfig > 0)
                {
                    var lignes = ImageUtil.SplitStringForLineRendering(WidthFromConfig, text, g2D, newFont);
                    for (var i = 0; i < lignes.Count; i++)
                    {
                        var rect = g2D.MeasureString(lignes[i], newFont);
                        DrawString(lignes[i], i + 1, newFont, _color, rect, WidthFromConfig, g2D);
                    }
                }
                else
                {
                    var rect = g2D.MeasureString(text, newFont); // TEXT SIZE
                    DrawString(text, 1, newFont, _color, rect, (int)rect.Width, g2D);
                }
                g2D.Flush();
                g2D.Dispose();
            }
        }

        private void DrawString(string text, int numeroLigne, Font font, Color color, SizeF rect, int width, Graphics g2D)
        {

            var textImage = new Bitmap(width, (int)rect.Height * 2);
            textImage.SetResolution(300, 300);
            var textGraphics = Graphics.FromImage(textImage);

            var xJustify = 0;
            if (Justify.Equals("right"))
            {
                xJustify = (int)(width - rect.Width);
            }
            else if (Justify.Equals("center"))
            {
                xJustify = (int)((width - rect.Width) / 2);
            }
            var brush = new SolidBrush(color);
            textGraphics.DrawString(text, font, brush, xJustify, (int)rect.Height);
            textGraphics.Flush();
            if (Rotation != 0)
                textImage = Rotate(textImage, Rotation);

            var tmp2 = textImage;
            textImage = ImageUtil.GetScaledInstance(tmp2, textImage.Width, textImage.Height);
            tmp2.Dispose();

            var newX = X;
            if (_center)
            {
                newX = Y - ((int)(rect.Width / 2f));
            }

            g2D.DrawImageUnscaled(textImage, newX, Y + (int)((numeroLigne - 1) * rect.Height * 1.3f));
        }

        private string GetText(IRespondent respondent)
        {
            var text = "";

            if (_questionChoiceList != null)
            {
                var nbrQuestionChoice = _questionChoiceList.Count;

                for (var i = 0; i < nbrQuestionChoice; i++)
                {
                    var questionChoice = _questionChoiceList[i];
                    var response = respondent.GetResponseText(questionChoice.GetQuestionNo(), questionChoice.GetChoiceNo());

                    if (string.IsNullOrEmpty(response)) continue;
                    if (i == 0)
                        text = response;
                    else
                        text += " " + response;
                }
            }

            if (_valueFromConfig != null && !_valueFromConfig.Equals(""))
                text += _valueFromConfig;
            if (_lowerCase)
                text = text.ToLower();
            if (_upperCase)
                text = text.ToUpper();
            return text;
        }

        private Font GetConfigFont()
        {

            if (_font == null)
                return null;

            if (_size == -1)
                _size = 14;

            return new Font(_font, _size, _style);
        }
        
        public int WidthFromConfig { get; set; }
        public string Justify { get; set; } = "left";
        public string Prefix { get; set; }
        public string Suffix { get; set; }

        public override AbstractLayer GetCopy()
        {
            return new TextLayer(this);
        }

        public override void Dispose()
        {
            // Rien à détruire.
        }

        protected override void OnLayerSettingsChanged()
        {
            // Rien
        }
    }
}
