using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    /// <summary>
    /// Affiche X images tirées de réponses textes du répondant courant.
    /// Ne fait aucune tentative de modification, ni de transformation scriptée, sur l'image. Sauf un redimensionnement pour préserver de la mémoire à l'affichage.
    /// </summary>
    [PublicAPI]
    public class ScreenDefinitionPreviews : IJsonConfig, IExternalFileConfig, IPaint, IRecyclable, IActivateScreen, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public IList<ScreenDefinitionPreviewImageBase> PreviewDefinitions { get; set; }

        public bool CheckForReloadEachActivation { get; set; }

        public string DefaultConfigFileName { get; } = "screen_def_preview_image.json";

        public string ConfigFileName { get; set; }

        public void Paint(Graphics g, bool debug = false)
        {
            foreach (var pd in PreviewDefinitions)
            {
                if (!pd.PreviewZone.HasValue) continue;

                ScreenUtil.DrawImage(g, pd._bmp, pd.PreviewZone.Value);
                if (debug) g.DrawRectangle(ScreenUtil.DebugBluePen, pd.PreviewZone.Value);
            }
        }

        public void Recycle(Control parent)
        {
            foreach (var pd in PreviewDefinitions)
            {
                pd.Dispose(); // Un recyclage ne fait que décharger les images en mémoire.
            }
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            foreach (var pd in PreviewDefinitions)
            {
                pd.ActivateExec(parent, respondent, CheckForReloadEachActivation, log);
            }
        }
        
        public void Dispose()
        {
            foreach (var pd in PreviewDefinitions)
            {
                pd?.Dispose();
            }
        }
        
    }

    public abstract class ScreenDefinitionPreviewImageBase : IDisposable
    {
        internal Image _bmp;
        public Rectangle? PreviewZone { get; set; }

        public void Dispose()
        {
            _bmp?.Dispose();
        }

        public abstract void ActivateExec(Control parent, IRespondent respondent, bool checkReload, Logger log);
    }

    public class ScreenDefinitionPreviewImageDynamicHeader : ScreenDefinitionPreviewImageBase
    {
        public string PreviewHeaderDataKey { get; set; }

        public int? DynamicHeaderValueIndex { get; set; }

        public override void ActivateExec(Control parent, IRespondent respondent, bool checkReload, Logger log)
        {
            if (!PreviewZone.HasValue)
            {
                log.Warn("No rectangle defined for preview image on question: " + (PreviewHeaderDataKey ?? "NOT_DEFINED"));
                return;
            }

            if (!checkReload)
            {
                if (_bmp != null) return; // Déjà chargé
            }

            var siz = PreviewZone.Value.Size;

            try
            {
                _bmp?.Dispose();

                var dhl = respondent.DynamicHeaders.FirstOrDefault(dh => dh.DataKey.Equals(PreviewHeaderDataKey));

                if (dhl == null)
                {
                    log.Error($"No dynamic header group found with Data Key value => {PreviewHeaderDataKey}");
                    return;
                }

                var idx = 0;
                if (DynamicHeaderValueIndex.HasValue && (dhl.DataValues.Count - 1) <= DynamicHeaderValueIndex.Value )
                {
                    idx = DynamicHeaderValueIndex.Value;
                }

                var txt = dhl.DataValues[idx]?.ValueRaw;

                if (!string.IsNullOrWhiteSpace(txt))
                {
                    using (var tmp = FileUtil.LoadBitmap(txt))
                    {
                        _bmp = tmp.Resize(siz.Width, siz.Height);
                    }
                }
                else
                {
                    log.Error($"No valid value found with DataKey/Index value => {PreviewHeaderDataKey}[{idx}]");
                }
            }
            catch (System.Exception e)
            {
                _bmp = null;
                log.Error(e, $"Could not load image from dynamic header to question '{PreviewHeaderDataKey}' for current respondent.");
            }
        }
    }

    public class ScreenDefinitionPreviewImageQuestion : ScreenDefinitionPreviewImageBase
    {
        public QuestionIdentifier PreviewQuestionIdentifier { get; set; }

        public override void ActivateExec(Control parent, IRespondent respondent, bool checkReload, Logger log)
        {
            if (!PreviewZone.HasValue)
            {
                log.Warn("No rectangle defined for preview image on question: " + (PreviewQuestionIdentifier?.ToString() ?? "NOT_DEFINED"));
                return;
            }

            if (!checkReload)
            {
                if (_bmp != null) return; // Déjà chargé
            }

            var siz = PreviewZone.Value.Size;

            try
            {
                _bmp?.Dispose();
                var txt = respondent.GetAnswerText(PreviewQuestionIdentifier);
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    using (var tmp = FileUtil.LoadBitmap(txt))
                    {
                        _bmp = tmp.Resize(siz.Width, siz.Height);
                    }
                }
            }
            catch (System.Exception e)
            {
                _bmp = null;
                log.Error(e, $"Could not load image from answer to question '{PreviewQuestionIdentifier}' for current respondent.");
            }
        }
    }
}