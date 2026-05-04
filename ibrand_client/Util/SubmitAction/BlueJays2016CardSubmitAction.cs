using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    public class BlueJays2016CardSubmitAction : CardSubmitAction
    {
        private readonly Brush _brush = new SolidBrush(Color.White);
        private Font _fontFront;
        private Font _fontBack;
        private readonly Point _frontPoint = new Point(227, 870);
        private readonly Point _backPoint = new Point(-780, 90);

        public override void DrawFrontImage(Respondent resp, PrivateFontCollection fontCollection, Bitmap canvas)
        {
            if (_fontFront == null)
            {
                if (!fontCollection.Families.Any())
                {
                    throw new System.Exception("No custom font found in configuration. Add the font name.");
                }
                var fontFam = fontCollection.Families.First();
                _fontFront = new Font(fontFam, 27f, FontStyle.Bold, GraphicsUnit.Pixel);
            }

            var respImageFile = resp.GetAnswerText(1);
            using (var respImage = FileUtil.LoadBitmap(respImageFile))
            {
                respImage.SetResolution(300, 300);
                var imageSize = ImagesSize ?? ImageUtil.CARD_PRINT_DEFAULT_INTERNAL_IMAGE_SIZE;

                using (var resized = respImage.Frame(imageSize.Width, imageSize.Height))
                {
                    resized.SetResolution(300, 300);
                    canvas.AddOverlay(resized);
                }
            }

            DrawImage(resp, canvas, "front_card.png", _fontFront, 0, _frontPoint);

            //ImageUtil.SaveToJpegWithOptions(canvas, 90, @"C:\Users\François Coupal\Documents\projets\2016\DIA-2016-01 MLB Sports Card\test\front.jpg");
        }

        public override void DrawBackImage(Respondent resp, PrivateFontCollection fontCollection, Bitmap canvas)
        {
            if (_fontBack == null)
            {
                if (!fontCollection.Families.Any())
                {
                    throw new System.Exception("No custom font found in configuration. Add the font name.");
                }
                var fontFam = fontCollection.Families.First();
                _fontBack = new Font(fontFam, 45f, FontStyle.Bold, GraphicsUnit.Pixel);
            }

            DrawImage(resp, canvas, "back_card.jpg", _fontBack, -90, _backPoint);

            //ImageUtil.SaveToJpegWithOptions(canvas, 90, @"C:\Users\François Coupal\Documents\projets\2016\DIA-2016-01 MLB Sports Card\test\back.jpg");
        }

        private void DrawImage(IRespondent resp, Bitmap canvas, string fileName, Font font, float rotation, Point textLocation)
        {
            var baseDir = GetTerminalDirectory;
            
            using (var g = Graphics.FromImage(canvas))
            {
                using (var bmp = FileUtil.LoadBitmap(baseDir + fileName))
                {
                    bmp.SetResolution(300, 300);
                    g.DrawImageUnscaled(bmp, 0, 0);
                }
            }

            var name = resp.GetAnswerText(4).ToUpperInvariant();
            var lastName = resp.GetAnswerText(5).ToUpperInvariant();
            canvas.AddTextToBitmap(name + " " + lastName, textLocation.X, textLocation.Y, false, _brush, font, rotation);
        }
    }
}