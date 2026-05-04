using System.Drawing;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Iris
{
    public partial class IdentifySideviewMetrics : IdentifyEyeMetrics
    {
         public IdentifySideviewMetrics(string backgroundImgSrc, ScreenConfig sc, PropertyFile pf)
            : base(backgroundImgSrc, sc, pf)
        {
            // Rien de plus
        }

         protected override void InitFiles()
         {
             if (_reset_bmp != null) _reset_bmp.Dispose();
             _reset_bmp = GetBitmap("reset.png");

             if (_previous_bmp != null) _previous_bmp.Dispose();
             _previous_bmp = GetBitmap("takenew.png");

             if (_next_bmp != null) _next_bmp.Dispose();
             _next_bmp = GetBitmap("done.png");

             if (_show_bmp != null) _show_bmp.Dispose();
             _show_bmp = GetBitmap("preview.png");
         }

        public override void SetActivated()
        {
            base.SetActivated();
            Reset();

            ShowZoomedImage(6);

            _distance_between_points.Focus();
        }

        protected override void Reset()
        {
            _point1 = null;
            _point2 = null;
            _point3 = null;
            _point1_mod = null;
            _point2_mod = null;
            _point3_mod = null;
            _distance_between_points.Text = "";
            ShowZoomedImage(6);
        }

        protected override void ShowNormalizedFile()
        {
            ShowNormalizedFileDo(6, _pf.GetProperty("center_coordinate_side"), _pf.GetPropertyDouble("pixel_mm"), _pf.GetPropertyDouble("angle_min"));
        }

        protected override void PupilSelectionDo(ref Point tmpPointMod, ref Point tmpPoint)
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
        }

        protected override bool SaveQuestions()
        {
            var success = false;
            /*if (_point1_mod.HasValue && _point2_mod.HasValue && !string.IsNullOrWhiteSpace(_distance_between_points.Text))
            {
                success = SaveAnswerToQuestion(8, 1, _point1_mod.Value.X + ", " + _point1_mod.Value.Y) &&
                    SaveAnswerToQuestion(9, 1, _point2_mod.Value.X + ", " + _point2_mod.Value.Y) &&
                    SaveAnswerToQuestion(10, 1, _distance_between_points.Text);
            }

            // Question optionnelle
            if (_point3_mod.HasValue)
            {
                success &= SaveAnswerToQuestion(12, 1, _point3_mod.Value.X + ", " + _point3_mod.Value.Y);
            }
*/

            return success;
        }
    }
}
