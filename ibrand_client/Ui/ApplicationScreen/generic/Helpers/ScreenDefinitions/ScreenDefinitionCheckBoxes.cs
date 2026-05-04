using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    /// <summary>
    /// Définitions des emplacement et comportement des zones où l'on doit cocher quelque chose.
    /// Une image de crochet est instanciée pour toutes les différentes zones, et apparaît quand une zone est activé.
    /// Chaque zone est soit activée, soit inactive, visuellement parlant. On peut également forcer une zone à être active avant de continuer.
    /// </summary>
    public class ScreenDefinitionCheckBoxes : IJsonConfig, IExternalFileConfig, IRecyclable, IActivateScreen, ISaveToQuestionnaire, IEvaluateScreenZones, IEvaluateDefinitionsState, IOnLoad, IDisposable, IPaint
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Textes destinés à l'usager par défaut pour une boîte de texte.
        /// </summary>
        public static readonly IReadOnlyDictionary<UserErrorEnum, IReadOnlyDictionary<string, string>> DefaultUserMessages;

        private ActivityLoggerService _activityLogger;

        static ScreenDefinitionCheckBoxes()
        {
            // Initialisation statique.
            // TODO À remplacer par un véritable système i18n

            var dico = new Dictionary<UserErrorEnum, IReadOnlyDictionary<string, string>>
            {
                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                {
                    {Constants.EN, "You must check this field to continue: {0}"}, {Constants.FR, "Vous devez cochez ce champ pour continuer: {0}"}
                }}
                ,{UserErrorEnum.System, new Dictionary<string, string>
                {
                    {Constants.EN, "System configuration error. Please contact an administrator."}, {Constants.FR, "Erreur de configuration système. Contactez un administrateur."}
                }}
            };

            DefaultUserMessages = dico;
        }

        public string DefaultConfigFileName => "screen_checkbox_definition.json";
        
        public string ConfigFileName { get; set; }
        
        /// <summary>
        /// Liste des définitions individuelles des zones à cocher.
        /// </summary>
        public IList<ScreenCheckOption> CheckboxDefinitions { get; set; }
        
        /// <summary>
        /// Indique que l'on veut sauvegarder les données au serveur immédiatement si la valeur change.
        /// </summary>
        public bool UpdateToServer { get; set; }

        [JsonIgnore]
        public FileInfo UpdateToServerDebugFile { get; set; }

        [JsonIgnore]
        public Bitmap CheckImageInstance { get; set; }

        /// <summary>
        /// Nom du fichier d'image qui contient le "check" qui doit être affiché quand une option est sélectionnée.
        /// </summary>
        public string CheckImageFileName { get; set; } = "checkmark.png";

        public string ActivitySaveLogName { get; set; }

        public void Recycle(Control parent)
        {
            if (CheckboxDefinitions == null) return;

            foreach (var zone in CheckboxDefinitions)
            {
                zone.Recycle();
            }
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            if (CheckImageInstance == null) return;

            var tdList = CheckboxDefinitions.ToList();
            if (!tdList.Any()) return;

            foreach (var def in tdList)
            {
                def.AssignInitialValue(respondent);
            }
        }

        public bool Save(ISaveLocalRespondentData saveTo, string activeLanguage)
        {
            if (CheckboxDefinitions == null || saveTo == null) return false;

            var tdList = CheckboxDefinitions.ToList();
            return !tdList.Any() || tdList.All(td => td.SaveToAllModels(saveTo, td.AnswerState));
        }

        public ScreenEvaluationResult EvaluateDefinitionsState(string activeLanguage)
        {
            if (CheckboxDefinitions == null) return null;

            var data = (from def in CheckboxDefinitions where !def.AnswerState && def.Mandatory select def.GetErrorResult(activeLanguage, UserErrorEnum.Mandatory, DefaultUserMessages)).ToList();

            var result = data.Any(r => r.Result == EvaluationResult.Incomplete) ? EvaluationResult.Incomplete :
                data.All(r => r.Result == EvaluationResult.Valid) ? EvaluationResult.Valid : EvaluationResult.Invalid;

            return new ScreenEvaluationResult
            {
                Result = result,
                UserMessages = data.SelectMany(d => d.UserMessages).ToList()
            };
        }

        public bool OnLoad(AbstractScreen screen)
        {
            if (screen == null || CheckboxDefinitions == null) return false;

            _activityLogger = ServicesContainer.GetService<ActivityLoggerService>();
            
            if (CheckImageInstance != null) return false;

            var baseDir = DirectoryUtil.GetLocalPhotoBoothDefaultAssetsDir();
            var file = FileUtil.FindFilePaths(baseDir, CheckImageFileName);
            if (file == null)
            {
                baseDir = screen.GetTerminalDirectory();
                file = FileUtil.FindFilePaths(baseDir, CheckImageFileName);
            }

            if (file == null) return false;

            CheckImageInstance = FileUtil.LoadBitmap(file.FullName);

            return CheckImageInstance != null;
        }

        public void Dispose()
        {
            CheckImageInstance?.Dispose();
        }

        public void Paint(Graphics g, bool debug = false)
        {
            if (CheckboxDefinitions == null) return;

            foreach (var zone in CheckboxDefinitions)
            {
                if (zone.AnswerState)
                {
                    ScreenUtil.DrawImage(g, CheckImageInstance, zone.CheckLocation ?? zone.Position.Location);
                }

                if (!debug) continue;
                if (zone.CheckLocation != null && CheckImageInstance != null)
                {
                    var c = new []
                    {
                        zone.CheckLocation.Value.X,
                        zone.CheckLocation.Value.Y,
                        zone.CheckLocation.Value.X + CheckImageInstance.Width,
                        zone.CheckLocation.Value.Y + CheckImageInstance.Height
                    };
                    g.DrawEllipse(ScreenUtil.DebugLightBluePen, c[0], c[1], c[2], c[3]);
                }
                g.DrawEllipse(ScreenUtil.DebugBluePen, zone.Position);
            }
        }

        public ScreenEvaluationResult EvaluateZones(AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod)
        {
            var hit = CheckboxDefinitions?.FirstOrDefault(data => MathUtil.InArea(x, y, data.Position));
            if (hit == null) return null;

            var isChanged = hit.Toggle();
            if (!isChanged) return new ScreenEvaluationResult();

            screen.Refresh();
            
            if (!(screen is GenericUpdateServerScreen upSCreen) || !UpdateToServer) return new ScreenEvaluationResult();

            // Vérifie si l'écran fait des envois vers le serveur à chaque changement d'état.
            Save(upSCreen, null);
                
            var resp = upSCreen.GetCurrentRespondentWithAnswers();
                
            // Charge données du serveur. Il faut garder le répondant en mémoire pour le modifier.
            // Pour l'envoi, il faut épurer le répondant pour enlever les question/valeurs dynamique en extra..
            var q = upSCreen.GetCurrentQuestionnaire();
            var sendResp = q.ActualizeRespondentData(resp);

            var uei = sendResp.ExternalUniqueIdentifier;

            if (!string.IsNullOrWhiteSpace(uei))
            {
                // Envoi des données au serveur.
                var rez = upSCreen.SendToServer(CancellationToken.None, sendResp, null, UpdateToServerDebugFile).GetAwaiter().GetResult();
                string logDetail;
                if (string.IsNullOrWhiteSpace(ActivitySaveLogName))
                {
                    logDetail = "Checkbox - Save: " + (rez ? "Success" : "Error");
                }
                else
                {
                    logDetail = ActivitySaveLogName;
                }
                _activityLogger.LogActivity(screen.TerminalId, ActivityLoggerActionType.DirectSave, logDetail);

                if (!rez)
                {
                    log.Error("Could not not save data to server.");
                    ScreenUtil.CheckAndShowUserMessages("Could not save data to server. Please investigate.");
                }
            }
            else
            {
                log.Error("Cannot send data to server if no external unique identifier is set to respondentin memory. Skipping.");
            }

            return new ScreenEvaluationResult();
        }
    }
}