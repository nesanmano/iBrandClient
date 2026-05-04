using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui
{
    [Description("Color Button Control")]
    [ToolboxBitmap(typeof(Button))]
    [Designer(typeof(ColorButtonDesigner))]
    public sealed class ColorButton : Control
    {
        private enum States
        {
            Normal,
            MouseOver,
            Clicked
        }

        public enum SmoothingQualities
        {
            None,
            HighSpeed,
            AntiAlias,
            HighQuality
        }

        public enum ButtonStyles
        {
            Rectangle,
            Ellipse
        }

        public enum GradientStyles
        {
            Horizontal,
            Vertical,
            ForwardDiagonal,
            BackwardDiagonal
        }

        // default values
        private bool _active = true;
        private States _state = States.Normal;

        private GradientStyles _gradientStyle = GradientStyles.Vertical;
        //private ButtonStyles _ButtonStyle = ButtonStyles.Rectangle;
        private SmoothingQualities _smoothingQuality = SmoothingQualities.AntiAlias;

        private Color _normalBorderColor = Color.FromArgb(255, 156, 170, 193);
        private Color _normalColorA = Color.FromArgb(255, 156, 170, 193); 
        private Color _normalColorB =  Color.White;

        private Color _hoverBorderColor = Color.FromArgb(255, 42, 58, 86);
        private Color _hoverColorA = Color.FromArgb(255, 42, 58, 86);
        private Color _hoverColorB = Color.White;

        public ColorButton()
        {
            // initiate button size and font
            Size = new Size(200, 40);
            Font = new Font("Arial", 8, FontStyle.Regular);
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.DoubleBuffer |
                ControlStyles.Selectable |
                ControlStyles.UserMouse,
                true
                );
        }

        [Description("Smoothing Quality"), Category("ColorButton")]
        public SmoothingQualities SmoothingQuality
        {
            get
            {
                return _smoothingQuality;
            }
            set
            {
                _smoothingQuality = value;
                Invalidate();
            }
        }

        [Description("Gradient Style"), Category("ColorButton")]
        public GradientStyles GradientStyle
        {
            get
            {
                return _gradientStyle;
            }
            set
            {
                _gradientStyle = value;
                Invalidate();
            }

        }

        /*[Description("Button Style"), Category("ColorButton")]
        public ButtonStyles ButtonStyle
        {
            get
            {
                return _ButtonStyle;
            }
            set
            {
                _ButtonStyle = value;
                this.Invalidate();
            }
        }*/

        [Description("The normal border color"), Category("ColorButton")]
        public Color NormalBorderColor
        {
            get
            {
                return _normalBorderColor;
            }
            set
            {
                _normalBorderColor = value;
                Invalidate();
            }

        }

        [Description("The hover border color"), Category("ColorButton")]
        public Color HoverBorderColor
        {
            get
            {
                return _hoverBorderColor;
            }
            set
            {
                _hoverBorderColor = value;
                Invalidate();
            }

        }

        [Description("Normal color A"), Category("ColorButton")]
        public Color NormalColorA
        {
            get
            {
                return _normalColorA;
            }
            set
            {
                _normalColorA = value;
                Invalidate();
            }
        }

        [Description("Normal color B"), Category("ColorButton")]
        public Color NormalColorB
        {
            get
            {
                return _normalColorB;
            }
            set
            {
                _normalColorB = value;
                Invalidate();
            }
        }

        [Description("Hover color A"), Category("ColorButton")]
        public Color HoverColorA
        {
            get
            {
                return _hoverColorA;
            }
            set
            {
                _hoverColorA = value;
                Invalidate();
            }
        }

        [Description("Hover color B"), Category("ColorButton")]
        public Color HoverColorB
        {
            get
            {
                return _hoverColorB;
            }
            set
            {
                _hoverColorB = value;
                Invalidate();
            }
        }

        [Description("Enable the button?"), Category("ColorButton")]
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                Invalidate();
            }
        }

        // to make sure the control is invalidated(repainted) when the text is changed
        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            LinearGradientBrush brush;
            LinearGradientMode mode;

            //
            // set SmoothingMode
            //
            switch (_smoothingQuality)
            {
                case SmoothingQualities.None:
                    e.Graphics.SmoothingMode = SmoothingMode.Default;
                    break;
                case SmoothingQualities.HighSpeed:
                    e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
                    break;
                case SmoothingQualities.AntiAlias:
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    break;
                case SmoothingQualities.HighQuality:
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    break;
            }

            //
            // set LinearGradientMode
            //
            switch (_gradientStyle)
            {
                case GradientStyles.Horizontal:
                    mode = LinearGradientMode.Horizontal;
                    break;
                case GradientStyles.Vertical:
                    mode = LinearGradientMode.Vertical;
                    break;
                case GradientStyles.ForwardDiagonal:
                    mode = LinearGradientMode.ForwardDiagonal;
                    break;
                case GradientStyles.BackwardDiagonal:
                    mode = LinearGradientMode.BackwardDiagonal;
                    break;
                default:
                    mode = LinearGradientMode.Vertical;
                    break;
            }

            var textSize = e.Graphics.MeasureString(Text, Font);
            var textX = Size.Width / 2 - (int)(textSize.Width / 2);
            var textY = Size.Height / 2 - (int)(textSize.Height / 2);
            var newRect = new Rectangle(ClientRectangle.X + 1, ClientRectangle.Y + 1,
                ClientRectangle.Width - 3, ClientRectangle.Height - 3);

            if (_active)
            {
                switch (_state)
                {
                    case States.Normal:
                        brush = new LinearGradientBrush(newRect, _normalColorA, _normalColorB, mode);
                        e.Graphics.FillRectangle(brush, newRect);
                        e.Graphics.DrawRectangle(new Pen(_normalBorderColor, 1), newRect);
                            
                        e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), textX, textY);
                        break;

                    /*case _States.MouseOver:
                        brush = new LinearGradientBrush(newRect, _HoverColorA, _HoverColorB, mode);
                        switch (_ButtonStyle)
                        {
                            case ButtonStyles.Rectangle:
                                e.Graphics.FillRectangle(brush, newRect);
                                e.Graphics.DrawRectangle(new Pen(_HoverBorderColor, 1), newRect);
                                break;
                            case ButtonStyles.Ellipse:
                                e.Graphics.FillEllipse(brush, newRect);
                                e.Graphics.DrawEllipse(new Pen(_HoverBorderColor, 1), newRect);
                                break;

                        }
                        e.Graphics.DrawString(this.Text, base.Font, new SolidBrush(base.ForeColor), textX, textY);
                        break;*/

                    case States.Clicked:
                        brush = new LinearGradientBrush(newRect, _hoverColorA, _hoverColorB, mode);
                        e.Graphics.FillRectangle(brush, newRect);
                        e.Graphics.DrawRectangle(new Pen(_hoverBorderColor, 1), newRect);

                        e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), textX + 1, textY + 1);
                        break;
                    case States.MouseOver:
                        break;
                }
            }
            else
            {
                brush = new LinearGradientBrush(newRect, _normalColorA, _normalColorB, mode);

                e.Graphics.FillRectangle(brush, newRect);
                e.Graphics.DrawRectangle(new Pen(_normalBorderColor, 1), newRect);

                e.Graphics.DrawString(Text, Font, new SolidBrush(_normalColorA), textX, textY);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (_active)
            {
                _state = States.Normal;
                Invalidate();
                base.OnMouseLeave(e);
            }
        }

        /*protected override void OnMouseEnter(System.EventArgs e)
        {
            if (_Active)
            {
                _State = _States.MouseOver;
                this.Invalidate();
                base.OnMouseEnter(e);
            }
        }*/

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_active)
            {
                _state = States.Normal;
                Invalidate();
                base.OnMouseUp(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (_active)
            {
                _state = States.Clicked;
                Invalidate();
                base.OnMouseDown(e);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            // prevent click when button is inactive
            if (_active)
            {
                if (_state == States.Clicked)
                {
                    base.OnClick(e);
                }
            }
        }
    }
}
