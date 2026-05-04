using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace NomadLogic.Common.Ui.Keyboard
{
    /// <summary>
    /// Classe de base pour un Label personnalisé, destiné à créer des graphiques personalisés pour les claviers.
    /// La propriété BackColor et FrontColor du Label original sont toujours utilisée. Pour le background ou effet de superposition dans le cas de BackColor,
    /// et pour le texte dans le cas de FrontColor.
    /// Fait avec les explications de ces sites web:
    /// https://www.codeproject.com/Articles/6913/Creating-a-professional-looking-GDI-drawn-custom-c
    /// https://web.archive.org/web/20121102092054/http://www.switchonthecode.com/tutorials/csharp-creating-rounded-rectangles-using-a-graphics-path
    /// </summary>
    [PublicAPI]
    public abstract class CustomPaintLabel : Label
    {
        /// <summary>
        /// Constructeur principal. Active les paramètre qui permettent on dessein personalisé sur chaque sous-classes.
        /// </summary>
        protected CustomPaintLabel() {
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Force la surcharge de "OnPaint" de la classe Label.
        /// </summary>
        /// <param name="e"></param>
        protected abstract void OnPaintDo(PaintEventArgs e);

        #region Overrides of Label
        
        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e) {
            OnPaintDo(e);
        }
        
        #endregion

        /// <summary>
        /// Classe d'information sur les variables partagée pour l'affichage.
        /// </summary>
        public CustomPaintLabelInfo CustomPaintLabelInfo { get; internal set; }

        /// <summary>
        /// Indique si le bouton est maintenu enfoncé, ce qui change son visuel. Habituellement réservé à des boutons de contrôle comme le CAPS_LOCK.
        /// </summary>
        public bool IsHeld { get; set; }
    }

    /// <summary>
    /// Classe de "wrapper" et aussi "Factory" pour créer une série d'instances de la classe personnalisée, en paratageant les informations communes.
    /// </summary>
    [PublicAPI]
    public class CustomPaintLabelInfo {

        private Type _type = typeof(GradientTextLabel);

        /// <summary>
        /// Le type de la classe de Label personnalisée. Par défaut, "GradientTextLabel".
        /// </summary>
        public Type LabelType {
            get => _type;
            set {
                if (typeof(CustomPaintLabel).IsAssignableFrom(value)) { _type = value; }
                else { throw new Exception("Cannot assign a Type which is not assignable to the 'NomadLogic.iBrand.Common.Ui.Keyboard.CustomPaintLabel' type."); }
            } 
        }

        /// <summary>
        /// Créer une instance du Label avec les informations correctes.
        /// </summary>
        /// <returns></returns>
        public CustomPaintLabel CreateInstance() {
            var instance = (CustomPaintLabel) Activator.CreateInstance(_type);
            instance.CustomPaintLabelInfo = this;
            return instance;
        }

        /// <summary>
        /// Méthode de base pour la génération de formes. Possède un mécanisme pour réutiliser les formes créées avec des paramètres similaires.
        /// </summary>
        /// <param name="label">Référence vers le CustomPaintLabel qui veut le chemin.</param>
        /// <returns>Une référence vers le GraphicsPath généré.</returns>
        public static GraphicsPath DefaultPath(CustomPaintLabel label) {

            if (label == null) return null;

            var size = label.Size;
            if (!reusablePaths.ContainsKey(size)) reusablePaths.Add(size, CustomPathFactory.CreateRoundedRectangle(label, 4)); 

            return reusablePaths[size];
        }

        private static readonly Dictionary<Size, GraphicsPath> reusablePaths = new Dictionary<Size, GraphicsPath>();

        /// <summary>
        /// Liste de couleurs données en paramètres. Chaque classe détermine si elle a besoin d'un nombre X de couleurs, et à quoi elles servent.
        /// Si la liste donnée en parmère est insuffisante, la classe la complétera avec des valeurs par défaut.
        /// </summary>
        public IList<Color> ColorPalette { get; set; } = new List<Color>();

        /// <summary>
        /// Méthode pour créer le chemin vectoriel du dessein. Pointe vers la méthode "DefaultPath".
        /// </summary>
        public Func<CustomPaintLabel, GraphicsPath> ShapeContainerMethod { get; set; } = DefaultPath;
        
        /// <summary>
        /// Pinceau pour le contour du chemin. Noir par défaut. N'est pas utilisé si on n'a pas de countour à dessiner.
        /// Peut être mis à null, pour indiquer qucun contour.
        /// </summary>
        //public Pen ContourPen { get; set; } = new Pen(Color.Black, 1);
        public Pen ContourPen { get; set; }

        /// <summary>
        /// Fonte pour le texte
        /// </summary>
        public Font Font { get; set; } = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);

        /// <summary>
        /// Style de Fonte pour le texte
        /// </summary>
        public FontStyle FontStyle { get; set; } = FontStyle.Regular;

        /// <summary>
        /// Indique une couleur spécifique quand on tient le cuseur appuyé sur une touche.
        /// </summary>
        public Color MouseDownColor { get; set; } = Color.FromArgb(128, Color.Gray);

        /// <summary>
        /// Indique une couleur spécifique quand on tient le cuseur appuyé sur une touche.
        /// </summary>
        public Color SelectedColor { get; set; } = Color.FromArgb(128, Color.Chocolate);

        /// <summary>
        /// Pinceau pour dessiner le texte.
        /// </summary>
        public Brush TextBrush = Brushes.Black;
    }

    /// <summary>
    /// Classe utilitaire pour contenir une méthode capable de créer un objet de type GraphicsPath, qui défini un rectangle avec chanfrein,
    /// ou un rectangle à coin arrondis.
    /// </summary>
    public static class CustomPathFactory {
        /// <summary>
        /// Méthode utilitaire pour construire une définition de rectangle chanfrein.
        /// </summary>
        /// <param name="label">L'instance de CustomPaintLabel.</param>
        /// <param name="radius">Rayon , en pixel, qui détermine la taille du chanfrein.</param>
        /// <param name="offset">Distance entre les côtés absolue de la forme. Rapetisse la forme vers le centre, mais peut être nécessaire pour que le tracé avec anticrénelage
        /// donne un bon résultat dans les contrainte d'espace du Label, si on utilise un contour. Par défaut, 4.</param>
        /// <returns>Une référence vers la définition du rectangle. Peut être utilisé dans les opérations de dessein GDI+.</returns>
        public static GraphicsPath CreateRoundedRectangle(CustomPaintLabel label, int radius, int offset = 4) {

            var x = offset;
            var y = offset;
            var w = label.Width - offset * 2;
            var h = label.Height - offset * 2;

            var p = new GraphicsPath();
            p.StartFigure();

            /*
            var d = radius * 2;
            var ml = w - d;
            var hl = h - d;

            var xr = x + radius;
            var xrml = xr + ml;
            var xw = x + w;

            var yr = y + radius;
            var yrhl = yr + hl;
            var yh = y + h;

            //Top Left Corner
            p.AddArc(xr, yr, d, d, 180, 270);

            //Top Edge
            p.AddLine(xr, y, xrml, y);

            //Top Right Corner
            p.AddArc(xrml, yr, d, d, 270, 0);

            //Right Edge
            p.AddLine(xw, yr, xw, yrhl);

            //Bottom Right Corner
            p.AddArc(xrml, yrhl, d, d, 0, 90);

            //Bottom Edge
            p.AddLine(xrml, yh, xr, yh);

            //Bottom Left Corner
            p.AddArc(xr, yrhl, d, d, 90, 180);

            //Left Edge
            p.AddLine(x, yrhl, x, yr);
            */

            
            var xw = x + w;
            var yh = y + h;
            var xwr = xw - radius;
            var yhr = yh - radius;
            var xr = x + radius;
            var yr = y + radius;
            var r2 = radius * 2;
            var xwr2 = xw - r2;
            var yhr2 = yh - r2;
            
            //Top Left Corner
            p.AddArc(x, y, r2, r2, 180, 90);

            //Top Edge
            p.AddLine(xr, y, xwr, y);

            //Top Right Corner
            p.AddArc(xwr2, y, r2, r2, 270, 90);

            //Right Edge
            p.AddLine(xw, yr, xw, yhr);

            //Bottom Right Corner
            p.AddArc(xwr2, yhr2, r2, r2, 0, 90);

            //Bottom Edge
            p.AddLine(xwr, yh, xr, yh);

            //Bottom Left Corner
            p.AddArc(x, yhr2, r2, r2, 90, 90);

            //Left Edge
            p.AddLine(x, yhr, x, yr);
            
            p.CloseFigure();
            return p;
        }
    }
}
