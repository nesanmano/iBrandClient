using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Iris
{
    public partial class IdentifyEyeMetrics : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool enable);

        private delegate void UiDelegate();
        private delegate void UiDelegateIbrand(bool success, Dictionary<string, string> results);

        private readonly Rectangle _nextArea;
        private readonly Rectangle _previous;
        private readonly Rectangle _reset;
        private readonly Rectangle _show;
        protected PictureBox _picture;

        protected Bitmap _previous_bmp;
        protected Bitmap _next_bmp;
        protected Bitmap _reset_bmp;
        protected Bitmap _show_bmp;
        protected TextBox _distance_between_points;

        private NumbersKeyboardControl _keyboard;

        protected float _scale;
        protected Pen _red_pen = new Pen(Color.Red, 10);
        protected Pen _blue_pen = new Pen(Color.Blue, 10);
        protected Point? _point1;
        protected Point? _point2;
        protected Point? _point3;
        protected Point? _point1_mod;
        protected Point? _point2_mod;
        protected Point? _point3_mod;
        protected PropertyFile _pf;

        public IdentifyEyeMetrics(string backgroundImgSrc, ScreenConfig sc, PropertyFile pf)
            : base(backgroundImgSrc, backgroundImgSrc, sc)
        {
            //PictureBox.MouseDown += MouseDownHandler;
            //PictureBox.Paint += PaintEvent;

            IsSaveOnThisPage = false;

            _picture = new PictureBox();
            _picture.Location = new Point(70, 70);
            _picture.Size = new Size(980, 980);
            _picture.Visible = true;
            _picture.MouseDown += PupilSelection;
            //_picture.Paint += PaintEventRedEye;
            Controls.Add(_picture);
            _picture.BringToFront();

            _pf = pf;

            _reset = new Rectangle(new Point(1780, 230), new Size(381, 120));
            _nextArea = new Rectangle(new Point(1780, 500), new Size(381, 120));
            _previous = new Rectangle(new Point(1122, 860), new Size(603, 162));
            _show = new Rectangle(new Point(1780, 360), new Size(381, 120));

            AddKeyboard();

            _distance_between_points = new TextBox();
            _distance_between_points.Location = new Point(1780, 100);
            _distance_between_points.Size = new Size(381, 100);
            _distance_between_points.Font = new Font(FontFamily.GenericSansSerif, 25);
            _distance_between_points.TextChanged += OnTextChanged;
            _distance_between_points.TextAlign = HorizontalAlignment.Center;
            Controls.Add(_distance_between_points);
            _distance_between_points.BringToFront();
            _distance_between_points.Focus();

            InitFiles();
        }

        protected virtual void InitFiles()
        {
            if (_reset_bmp != null) _reset_bmp.Dispose();
            _reset_bmp = GetBitmap("reset.png");

            if (_previous_bmp != null) _previous_bmp.Dispose();
            _previous_bmp = GetBitmap("takenew.png");

            if (_next_bmp != null) _next_bmp.Dispose();
            _next_bmp = GetBitmap("next.png");

            if (_show_bmp != null) _show_bmp.Dispose();
            _show_bmp = GetBitmap("preview.png");
        }

        /*private void PaintEventRedEye(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (_point1.HasValue)
            {
                e.Graphics.DrawEllipse(_red_pen, new Rectangle(_point1.Value.X, _point1.Value.Y, 1, 1));
            }

            if (_point2.HasValue)
            {
                e.Graphics.DrawEllipse(_red_pen, new Rectangle(_point2.Value.X, _point2.Value.Y, 1, 1));
            }

            if (_point3.HasValue)
            {
                e.Graphics.DrawEllipse(_blue_pen, new Rectangle(_point3.Value.X, _point3.Value.Y, 1, 1));
            }
        }
*/

        private void PupilSelection(object sender, MouseEventArgs e)
        {
            var offsetX = (_picture.Width / 4) / _scale;
            var offsetY = (_picture.Height / 4) / _scale;
            var modX = e.X / _scale / 2;
            var modY = e.Y / _scale / 2;

            var tmpPointMod = new Point((int)(offsetX + modX), (int)(offsetY + modY));
            var tmpPoint = new Point(e.X, e.Y);

            PupilSelectionDo(ref tmpPointMod, ref tmpPoint);

            Refresh();
            ResetTimer();
        }

        protected virtual void PupilSelectionDo(ref Point tmpPointMod, ref Point tmpPoint)
        {
            if (_point1 == null)
            {
                _point1 = tmpPoint;
                _point1_mod = tmpPointMod;
            }
            else if (_point2 == null)
            {
                _point2 = tmpPoint;
                _point2_mod = tmpPointMod;
            }
            else if (_point3 == null)
            {
                _point3 = tmpPoint;
                _point3_mod = tmpPointMod;
            }
        }

        public void OnTextChanged(object sender, EventArgs e)
        {
            if (_activated)
            {
                Refresh();
                ResetTimer();
            }
        }

        private void AddKeyboard()
        {
            AddKeyboardToControls(1124, 230, 600, 600);
        }

        /*public new void PaintEvent(object sender, PaintEventArgs e)
        {
            base.PaintEvent(sender, e);

            if (_point1.HasValue && _point2.HasValue && !string.IsNullOrWhiteSpace(_distance_between_points.Text))
            {
                e.Graphics.DrawImageUnscaled(_next_bmp, _nextArea);
                e.Graphics.DrawImageUnscaled(_show_bmp, _show);
            }

            e.Graphics.DrawImageUnscaled(_previous_bmp, _previous);

            e.Graphics.DrawImageUnscaled(_reset_bmp, _reset);
        }
*/

        public override void Recycle()
        {
            base.Recycle();

            _picture.Image?.Dispose();

            Reset();
        }

        protected virtual void Reset()
        {
            _point1 = null;
            _point2 = null;
            _point3 = null;
            _point1_mod = null;
            _point2_mod = null;
            _point3_mod = null;
            _distance_between_points.Text = "";
            ShowZoomedImage(1);
        }

        /*protected override void MouseDownHandler(object sender, MouseEventArgs e)
        {
            base.MouseDownHandler(sender, e);

            if (InArea(e.X, e.Y, _nextArea) && SaveQuestions())
            {
                Next();
            }
            else if (InArea(e.X, e.Y, _previous))
            {
                Back();
            }
            else if (InArea(e.X, e.Y, _reset))
            {
                Reset();
                Refresh();
            }
            else if (InArea(e.X, e.Y, _show) && _point1_mod.HasValue && _point2_mod.HasValue && !string.IsNullOrWhiteSpace(_distance_between_points.Text))
            {
                _point1 = null;
                _point2 = null;
                _point3 = null;
                ShowNormalizedFile();
                ResetTimer();
            }
        }
*/

        protected virtual void ShowNormalizedFile()
        {
            ShowNormalizedFileDo(1, _pf.GetProperty("center_coordinate_face"), _pf.GetPropertyDouble("pixel_mm"), _pf.GetPropertyDouble("angle_min"));
        }

        protected void ShowNormalizedFileDo(int qNo, string center, double pixelMm, double minAngle)
        {
            try
            {
                using (var ori = new Bitmap(Image.FromFile(GetTextAnswerFromQuestion(qNo, 1))))
                {
                    var centerP = StringUtil.PointFromStringDefinition(center);

                    // On normalize
                    using (var res = ori.NormalizeToStaticCoordinate(
                        _point1_mod.Value,
                        _point2_mod.Value,
                        _point3_mod,
                        int.Parse(_distance_between_points.Text),
                        centerP,
                        pixelMm,
                        minAngle))
                    {
                        using (var cropped = res.Frame(ori.Width, ori.Height))
                        {
                            using (var over = new Bitmap(GetNoneLanguageBitmap("cadre.png")))
                            {
                                // On met maintenant le cadre de lunette test
                                var xOffset = centerP.X - (over.Width / 2);
                                var yOffset = centerP.Y - (over.Height / 2);

                                var tmp = new Bitmap(cropped);
                                tmp.AddOverlay(over, xOffset, yOffset);

                                _picture.Image.Dispose();
                                _picture.Image = tmp;
                                _picture.SizeMode = PictureBoxSizeMode.Zoom;
                                _picture.Refresh();

                                res.Save(ImageFormat.Png, 9, DirectoryUtil.GetLocalApplicationWorkDir() + "normalized.png");
                                cropped.Save(ImageFormat.Png, 9, DirectoryUtil.GetLocalApplicationWorkDir() + "cropped.png");
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }
        }

        protected virtual bool SaveQuestions()
        {
            var success = false;
            /*if (_point1_mod.HasValue && _point2_mod.HasValue && !string.IsNullOrWhiteSpace(_distance_between_points.Text))
            {
                success = SaveAnswerToQuestion(3, 1, _point1_mod.Value.X + ", " + _point1_mod.Value.Y) &&
                    SaveAnswerToQuestion(4, 1, _point2_mod.Value.X + ", " + _point2_mod.Value.Y) &&
                    SaveAnswerToQuestion(5, 1, _distance_between_points.Text);
            }

            // Question optionnelle
            if (_point3_mod.HasValue)
            {
                success &= SaveAnswerToQuestion(11, 1, _point3_mod.Value.X + ", " + _point3_mod.Value.Y);
            }*/

            return success;
        }

        public override void SetActivated()
        {
            base.SetActivated();
            Reset();

            ShowZoomedImage(1);

            _distance_between_points.Focus();
        }

        protected void ShowZoomedImage(int qNo)
        {
            var text = GetTextAnswerFromQuestion(qNo, 1);

            if (!string.IsNullOrWhiteSpace(text))
            {
                var tmp = Image.FromFile(text);
                var bmp = new Bitmap(_picture.Width, _picture.Height);
                bmp.SetResolution(tmp.HorizontalResolution, tmp.VerticalResolution);

                _scale = bmp.Width / (float)tmp.Width;

                var newWidth = tmp.Width * _scale * 2;
                var newHeight = tmp.Height * _scale * 2;

                var g = Graphics.FromImage(bmp);
                var destRect = new RectangleF(-newWidth / 4, -newHeight / 4, newWidth, newHeight);
                g.DrawImage(tmp, destRect);

                _picture.Image = bmp;
                _picture.SizeMode = PictureBoxSizeMode.Normal;
            }
        }

        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;
            InitFiles();
            return true;
        }
    }
}
