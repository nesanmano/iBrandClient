using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;
using NomadLogic.Multimedia.Cli.ImageMagickCLI;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    public abstract class ScreenDefinitionSelectionBase<T> : IJsonConfig, IExternalFileConfig, IChangeLanguage, IRecyclable, IPaint, IEvaluateScreenZones, IImageInitialisable where T : AnswerZone
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Textes destinés à l'usager par défaut pour des zones de sélection.
        /// </summary>
        public static readonly IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> DefaultUserMessages;

        static ScreenDefinitionSelectionBase()
        {
            // Initialisation statique.
            // TODO À remplacer par un véritable système i18n

            var dico = new Dictionary<UserErrorEnum, IReadOnlyDictionary<string, string>>
            {
                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                {
                    {Constants.EN, ""}, {Constants.FR, "Vous devez choisir l'une des options présentées."}
                }}
                ,{UserErrorEnum.System, new Dictionary<string, string>
                {
                    {Constants.EN, "System configuration error. Please contact an administrator."}, {Constants.FR, "Erreur de configuration système. Contactez un administrateur."}
                }}
            };

            DefaultUserMessages = dico;
        }

        /// <summary>
        /// Dictionnaires des messages d'erreurs spécifiques liés au groupe d'actions.
        /// </summary>
        public IDictionary<UserErrorEnum, IDictionary<string, string>> UserMessages { get; set; }

        public abstract string DefaultConfigFileName { get; set; } // Force l'implémentation dans les classe non-abstraites

        public string ConfigFileName { get; set; }

        public virtual IList<T> SelectionZones { get; set; }

        [JsonIgnore]
        public T CurrentlySelectedZone { get; set; }

        [JsonIgnore]
        public DirectoryInfo ThumbnailDirectory { get; set; }

        /// <summary>
        /// Indique qu'il faut absolument avoir choisi une sélection possible avant de passer à un prochain écran.
        /// </summary>
        public bool Mandatory { get; set; }
        
        public void Dispose()
        {
            if (SelectionZones == null) return;
            foreach (var az in SelectionZones)
            {
                az?.Dispose();
            }
        }

        public void Paint(Graphics g, bool debug = false)
        {
            if (SelectionZones == null) return;
            foreach (var option in SelectionZones)
            {
                ScreenUtil.DrawImage(g, option.BackgroundImageInstance, option.Zone);
                if (debug) g.DrawRectangle(ScreenUtil.DebugBluePen, option.Zone);
            }

            if (CurrentlySelectedZone?.SelectionActivatedImage == null) return;
            var p = CurrentlySelectedZone.SelectionActivatedImageLocation ?? CurrentlySelectedZone.Zone.Location;
            ScreenUtil.DrawImage(g, CurrentlySelectedZone.SelectionActivatedImage, p);
            if (debug)
            {
                ScreenUtil.DrawDebugRectangle(g, ScreenUtil.DebugLightBluePen, CurrentlySelectedZone?.SelectionActivatedImageLocation, CurrentlySelectedZone?.SelectionActivatedImage?.Size);
            }
        }

        public void Recycle(Control parent)
        {
            CurrentlySelectedZone = null;
        }

        public ScreenEvaluationResult EvaluateZones(AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod)
        {
            var hit = SelectionZones?.FirstOrDefault(data => MathUtil.InArea(x, y, data.Zone));
            if (hit == null) return null;

            var results = new List<ScreenEvaluationResult>();

            if (hit.AnswersUponSelection?.Any() ?? false)
            {
                results.Add(new ScreenEvaluationResult(screen.SaveAnswers(hit.AnswersUponSelection)));
            }

            AddActions(screen, hit, results);

            var sco = hit.PageChangeOptions.Evaluate(screen.GetCurrentRespondentWithAnswers());
            if (sco != null)
            {
                results.Add(sco.Cancel ? new ScreenEvaluationResult(screen.FirstScreen) : new ScreenEvaluationResult(() => screen.Next(sco.Step)));
            }

            return ScreenEvaluationResult.AggregateResults(results);
        }

        /// <summary>
        /// Ajoute des actions spéciales générées par les classe d'implémentation.
        /// </summary>
        /// <param name="screen">l'écran qui exécute l'action.</param>
        /// <param name="hit">La zone activée</param>
        /// <param name="addTo">La liste d'actions à exécuter.</param>
        protected virtual void AddActions(AbstractScreen screen, T hit, List<ScreenEvaluationResult> addTo)
        {
            // Rien par défaut.
        }

        public bool InitImage(string baseDir, string languageCode)
        {
            if (SelectionZones == null) return true;

            // TODO Ajouter i18n

            try
            {
                return SelectionZones.All(o => LoadImagesForZone(baseDir, o));
            }
            catch (System.Exception e)
            {
                log.Error(e);
                return false;
            }
        }

        protected bool LoadImagesForZone(string baseDir, T option)
        {
            var path = FileUtil.FindFilePaths(baseDir, option.BackgroundImageFilePath);
            if (path?.Exists ?? false)
            {
                if (ThumbnailDirectory == null || !ImageMagickCliUtil.LoadImageFromThumbnail(path, ThumbnailDirectory, new Size(), out var bmp))
                {
                    bmp = FileUtil.LoadBitmap(path.FullName);
                }

                if (bmp == null) return false;
                option.BackgroundImageInstance = bmp;
            }

            path = FileUtil.FindFilePaths(baseDir, option.SelectionActivatedImagePath);
            if (!(path?.Exists ?? false)) return true;
            
            if (ThumbnailDirectory == null || !ImageMagickCliUtil.LoadImageFromThumbnail(path, ThumbnailDirectory, new Size(), out var bmp2))
            {
                bmp2 = FileUtil.LoadBitmap(path.FullName);
            }
            if (bmp2 == null) return false;
            option.SelectionActivatedImage = bmp2;

            return true;
        }

        public virtual void ChangeLanguage(AbstractScreen screen, string languageCode)
        {
            // Rien par défaut
        }
    }
}