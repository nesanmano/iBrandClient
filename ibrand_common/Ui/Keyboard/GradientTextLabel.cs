using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui.Keyboard
{
    /// <summary>
    /// Classe d'extension de Label avec un style graphique unique, fait sur mesure.
    /// A besoin de deux couleur pour faire un gradient.
    /// </summary>
    public class GradientTextLabel : CustomPaintLabel
    {
        /// <summary>
        /// Constructeur principal.
        /// </summary>
        public GradientTextLabel()
        {
            _solidBrush = new SolidBrush(Color.Transparent);

            _textBrush = new SolidBrush(ForeColor);
            ForeColorChanged += (sender, args) => {
                _textBrush = new SolidBrush(ForeColor);
            };
        }
        
        #region Overrides of Label

        /// <inheritdoc />
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            _solidBrush = new SolidBrush(CustomPaintLabelInfo.MouseDownColor);
            Refresh();
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _solidBrush = Brushes.Transparent;
            Refresh();
        }

        /// <inheritdoc />
        protected override void OnPaintDo(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (_font == null) {
                _font = new Font(CustomPaintLabelInfo.Font, CustomPaintLabelInfo.FontStyle);
            }

            if (CustomPaintLabelInfo.ShapeContainerMethod != null && _shape == null) {
                _shape = CustomPaintLabelInfo.ShapeContainerMethod(this);
            }

            if (_shape != null) {
                g.SetClip(_shape);
            }

            // Fond
            if (_gradientBrush == null) {
                var p1 = new Point(0,0);
                var p2 = new Size(Width,Height);
                _gradientBrush = CustomPaintLabelInfo.ColorPalette.Count >= 2
                                     ? new LinearGradientBrush(new Rectangle(p1, p2), CustomPaintLabelInfo.ColorPalette[0], CustomPaintLabelInfo.ColorPalette[1], 90)
                                     : new LinearGradientBrush(new Rectangle(p1, p2), Color.Azure, Color.Gray, 90);
                
            }

            if (_heldBrush == null) { _heldBrush = new SolidBrush(CustomPaintLabelInfo.SelectedColor); }

            g.FillRectangle(_gradientBrush, 0, 0, Width, Height);

            if (IsHeld) g.FillRectangle(_heldBrush, 0, 0, Width, Height);

            // Texte
            var textBrush = CustomPaintLabelInfo.TextBrush ?? _textBrush;
            var sizeText = g.MeasureString(Text, _font);
            g.DrawString(Text, _font, textBrush, (Width - sizeText.Width)/2, (Height - sizeText.Height)/2);

            // Overlay de sélection (OnMouseDown)
            g.FillRectangle(_solidBrush, 0, 0, Width, Height);

            if (_shape == null) return;

            if (CustomPaintLabelInfo.ContourPen != null)
            {
                g.DrawPath(CustomPaintLabelInfo.ContourPen, _shape);
            }
            g.ResetClip();
        }
        
        private LinearGradientBrush _gradientBrush;
        private Brush _solidBrush;
        private Brush _heldBrush;
        private SolidBrush _textBrush;
        private GraphicsPath _shape;
        private Font _font;

        #endregion
    }
}