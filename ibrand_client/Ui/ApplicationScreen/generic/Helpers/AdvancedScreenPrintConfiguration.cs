using System;
using System.Drawing;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Cette class permet d'afficher le decompte entre les impressions
    /// Il contint aussi certains propriétés utilisés par le printer comme le Timeout
    /// </summary>
    [Obsolete]
    public class AdvancedScreenPrintConfiguration : IDisposable
    {
        // Définie le point où l'on doit dessiner le compteur.
        // Point est un struct du coup les valeurs de X et Y sont à zero. Appelé un Empty Point
        // Il faut que cette propriété et le FontDefinitionInfo soit pas null pour afficher une valeur.
        public Point Point { get; set; }

        public int MaxPrintOuts { get; set; } = -1;

        // Définie le font avec la couleur qui est utilisé par le brush, un font par defaut est assigné par défaut.
        public FontDefinitionInfo FontDefinitionInfo { get; set; }

        //Spécifie le wait time entre les impressions
        public int PrintTimeout { get; set; } = 500; 


        private SolidBrush _solidBrush;

        private Font _font;

        
        public void Draw(Graphics e, object index)
        {

            if (Point.IsEmpty || FontDefinitionInfo == null) return;

            if(_font == null)
            {
                _font = StaticCustomFontRepository.CreateCustomFont(FontDefinitionInfo, FontDefinitionInfo.Size);
            }

            if (_solidBrush == null)
            {
                _solidBrush = new SolidBrush(FontDefinitionInfo.Color);
            }

            e.DrawString(index.ToString(),_font,_solidBrush,Point.X,Point.Y);
        }
        
        public void Dispose()
        {
            _solidBrush.Dispose();
            _font = null;
        }
    }
}
