using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.PrintStation.Ui.generic;

// ReSharper disable InconsistentNaming

namespace NomadLogic.PrintStation.Ui.custom.Mosaique
{
    public class BackgroundSelection_MAS : AbstractPrintStationScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        
        private readonly Rectangle? _cancelArea;
        private AnswerZone _chosenZone;
        public bool HasAnswer { get; private set; }

        public List<AnswerZone> Zones { get; set; } = new List<AnswerZone>();

        public BackgroundSelection_MAS(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc, Rectangle? cancelArea)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            _cancelArea = cancelArea;
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();
            HasAnswer = false;
            _chosenZone = null;
        }

        /// <inheritdoc/>
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;
            
            if (InArea(x, y, _cancelArea))
            {
                Close();
                return false;
            }

            foreach (var z in Zones)
            {
                if (!InArea(x, y, z.Zone)) continue;

                _chosenZone = z;
                
                TransformAndPrintPicture();
                return false;
            }

            return true;
        }
        
        private void TransformAndPrintPicture()
        {
            // 1- Sauvegarde des réponses
            if (!SaveAnswers(_chosenZone.AnswersUponSelection))
            {
                throw new Exception("Could not save answers in questionnaire.");
            }
            Next();
        }
        
        /// <inheritdoc/>
        public override void SetActivated()
        {
            base.SetActivated();
            log.Trace("SetActivated Called");
        }

        protected override void OnLoad()
        {
            var basePath = GetTerminalDirectory();

            foreach (var z in Zones)
            {
                var path = FileUtil.FindFirstFilePath(basePath, z.BackgroundImageFilePath);
                var bmp = FileUtil.LoadBitmap(path.FullName);
                if (bmp == null)
                {
                    log.Error($@"Could not load bitmap: {basePath}\{z.BackgroundImageFilePath}");
                    continue;
                }

                z.BackgroundImageInstance = bmp;
            }
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);

            foreach (var z in Zones.Where(z => z.BackgroundImageInstance != null))
            {
                ScreenUtil.DrawImage(g, z.BackgroundImageInstance, z.Zone.Location);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var z in Zones)
            {
                z.Dispose();
            }
        }
    }
}
