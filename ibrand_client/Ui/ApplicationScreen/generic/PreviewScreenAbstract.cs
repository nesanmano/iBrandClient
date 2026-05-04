using System.Drawing;
using JetBrains.Annotations;
using NLog;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    [PublicAPI]
    public abstract class PreviewScreenAbstract : AbstractScreen
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected readonly Rectangle? _nextArea;
        protected readonly Rectangle? _backArea;
        protected readonly Rectangle? _closeArea;
        protected bool _blockMouseEvents;

        protected PreviewScreenAbstract(string backgroundImgEnSrc, string backgroundImageFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle? nextZone, Rectangle? backZone, int fromTop, Size previewSize)
            : this(backgroundImgEnSrc, backgroundImageFrSrc, sc, cancelZone, nextZone, backZone, new Point(-1, fromTop), previewSize)
        {
            // rien d'autre
        }

        protected PreviewScreenAbstract(string backgroundImgEnSrc, string backgroundImageFrSrc, ScreenConfig sc,
            Rectangle? cancelZone, Rectangle? nextZone, Rectangle? backZone, Point previewLocation, Size previewSize)
            : base(backgroundImgEnSrc, backgroundImageFrSrc, sc)
        {
            _backArea = backZone;
            _nextArea = nextZone;
            _closeArea = cancelZone;

            AddPreviewControl(previewSize, previewLocation);
        }

        /// <summary>
        /// Indique les options de changement de page sur l'action "Next".
        /// </summary>
        public ScreenChangeOptions NextPageOptions { get; set; } = ScreenChangeOptions.GenerateBasicNextOptions();

        /// <summary>
        /// Indique les options de changement de page sur l'action "Back".
        /// </summary>
        public ScreenChangeOptions BackPageOptions { get; set; } = ScreenChangeOptions.GenerateBasicBackOptions();
        
        protected abstract void AddPreviewControl(Size previewSize, Point previewLocation);

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            if (_blockMouseEvents) return false;

            if (InArea(x, y, _nextArea))
            {
                ActionOnNext();
                return false;
            }

            if (InArea(x, y, _backArea))
            {
                ActionOnBack();
                return false;
            }

            if (InArea(x, y, _closeArea))
            {
                ActionOnClose();
                return false;
            }

            return true;
        }

        protected virtual void ActionOnNext()
        {
            var step = 1;
            
            var tmp = NextPageOptions?.Evaluate(GetCurrentRespondentWithAnswers());
            if (tmp != null) step = tmp.Step;
            
            Next(step);
        }

        protected virtual void ActionOnBack()
        {
            var step = -1;
            
            var tmp = BackPageOptions?.Evaluate(GetCurrentRespondentWithAnswers());
            if (tmp != null) step = tmp.Step;
            
            // On n'utilise pas back car on va vers le nouveau paradigme où l'on utilise une valeur négative.
            Next(step);
        }

        protected virtual void ActionOnClose()
        {
            FirstScreen();
        }
    }
}
