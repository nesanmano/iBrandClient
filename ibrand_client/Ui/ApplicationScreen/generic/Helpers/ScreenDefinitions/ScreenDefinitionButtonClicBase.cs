using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.ActivityLogger;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions
{
    [PublicAPI]
    public abstract class ScreenDefinitionButtonClicBase : IOnLoad, IDisposable, IActivateScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected Button _btn;
        protected IRespondent _curResp;
        protected AbstractScreen _screen;
        private ActivityLoggerService _activityLogger;
        private bool _canExecute;

        /// <summary>
        /// Information à mettre dans le journal d'activité pour les événements de base.
        /// </summary>
        public string ActivitySaveLogName { get; set; }

        /// <summary>
        /// Source de données venant du questionnaire sur laquel agir.
        /// </summary>
        [JsonRequired]
        public DataModelBase DataModelSource { get; set; }

        /// <summary>
        /// La zone d'activité pour l'usager.
        /// </summary>
        [JsonRequired]
        public Rectangle ButtonZone { get; set; }

        /// <summary>
        /// Le nom du fichier d'Image pour le bouton d'impression.
        /// </summary>
        public string ButtonImageFileName { get; set; }

        /// <summary>
        /// Délais artificiel (en milisecondes) pour éviter un spam de la fonction d'impression. Défaut = 1000
        /// </summary>
        public int ArtificialDelayMili { get; set; } = 1000;

        /// <summary>
        /// Couleur sous le bouton (si il y a de la transparence dans le fichier original).
        /// </summary>
        public Color? ButtonBackgroundColor { get; set; }

        /// <summary>
        /// Liste de valeurs fixes à sauvegarder dans le questionnaire sur activation de ce bouton.
        /// </summary>
        public IList<(DataModelBase, object)> SaveFixedDataOnActivation { get; set; }
        
        /// <summary>
        /// Indique que l'on veut provoquer une sauvegarde sur le serveur quand on appuis sur ce bouton.
        /// </summary>
        public bool UpdateToServer { get; set; }

        /// <summary>
        /// Fichier de débogage pour les requêtes de sauvegarde.
        /// </summary>
        [JsonIgnore]
        public FileInfo UpdateToServerDebugFile { get; set; }

        public bool OnLoad(AbstractScreen screen)
        {
            _activityLogger = ServicesContainer.GetService<ActivityLoggerService>();
            // Création du bouton
            var imgPath = screen.GetTerminalDirectory() + ButtonImageFileName;
            var imgBmp = (Bitmap)FileUtil.LoadImage(imgPath).Clone();

            _btn = new Button
            {
                Location = ButtonZone.Location,
                Size = ButtonZone.Size,
                Image = imgBmp,
                BackColor = ButtonBackgroundColor ?? Color.Transparent, // Transparence ne fonctionne pas de toute façon Voir: https://stackoverflow.com/questions/1086621/drawing-a-transparent-button
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = {BorderSize = 0}
            };
            screen.Controls.Add(_btn);
            _btn.BringToFront();

            _btn.Click += (sender, args) => BtnOnClick();

            _screen = screen;
            _canExecute = true;
            return true;
        }

        private void BtnOnClick()
        {
            if (!_screen?.IsActivated() ?? true) return;
            // Si ne fonctionne pas pour limiter à un la limite d'exécution, revenir à un système de task avec jeton.
            if (_canExecute == false) return;

            try
            {
                _canExecute = false;

                log.Trace("Begin thread sequence.");
                var t = new Thread(ExecuteTemplate);
                t.SetApartmentState(ApartmentState.STA);
                t.Start();

                if (ArtificialDelayMili > 0)
                {
                    Task.Delay(ArtificialDelayMili).GetAwaiter().GetResult(); // Pour éviter les spams.
                }
            }
            finally
            {
                _canExecute = true;
            }
            
            
        }

        private void ExecuteTemplate()
        {
            try
            {
                var allGood = PrepareExecution();

                if (!allGood) return;

                allGood = ExecuteMainLogic();
                
                if (!allGood)
                {
                    log.Warn("Execution did not sucessfully complete.");
                    return;
                }

                allGood = CleanupExecution();
                
                if (!allGood)
                {
                    log.Warn("Cleanup did not sucessfully complete.");
                    return;
                }

                var provokeRefresh = false;
                
                if (SaveFixedDataOnActivation != null)
                {
                    foreach (var (dataModel, val) in SaveFixedDataOnActivation)
                    {
                        switch (val)
                        {
                            case bool valBool:
                                dataModel.SaveData(_screen, valBool);
                                break;
                            case IEnumerable<string> valCol:
                                dataModel.SaveData(_screen, valCol);
                                break;
                            default:
                                dataModel.SaveData(_screen, val.ToString());
                                break;
                        }

                        provokeRefresh = true;

                        log.Trace("Saved data to disk: Value={1}, Model={0}", dataModel, val.ToString());
                    }
                }

                if (UpdateToServer && _screen is GenericUpdateServerScreen uptScreen)
                {
                    var resp = uptScreen.GetCurrentRespondentWithAnswers();
                    resp = uptScreen.GetCurrentQuestionnaire().ActualizeRespondentData(resp);

                    if (string.IsNullOrWhiteSpace(resp.ExternalUniqueIdentifier))
                    {
                        log.Error("Respondent does not have a unique identifier. Cannot save to server. SKipping save sequence.");
                    }
                    else
                    {
                        var rez = uptScreen.SendToServer(CancellationToken.None, resp, null, UpdateToServerDebugFile).GetAwaiter().GetResult();
                        string logDetail;
                        if (string.IsNullOrWhiteSpace(ActivitySaveLogName))
                        {
                            logDetail = "Save to server: " + (rez ? "Success" : "Error");
                        }
                        else
                        {
                            logDetail = ActivitySaveLogName;
                        }
                        _activityLogger.LogActivity(_screen.TerminalId, ActivityLoggerActionType.DirectSave, logDetail);

                        if (!rez)
                        {
                            log.Error("Could not save to server.");
                            ScreenUtil.CheckAndShowUserMessages("Could not save to server.");
                        }
                    }

                    provokeRefresh = true;
                }

                if (provokeRefresh)
                {
                    _screen.SafeCall(() =>
                    {
                        _screen.SetActivated();
                        _screen.Refresh();
                    });
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Exception occured in execution sequence.");
            }
        }

        /// <summary>
        /// Ce qui s'exécute après l'exécution principale.
        /// </summary>
        protected virtual bool CleanupExecution()
        {
            return true;
        }

        /// <summary>
        /// Préparatifs/Vérifications préalables de l'exécution principale.
        /// </summary>
        protected virtual bool PrepareExecution()
        {
            return true; // Rien de spécial par défaut.
        }

        /// <summary>
        /// La logique d'exécution principale.
        /// </summary>
        protected abstract bool ExecuteMainLogic();

        public void Dispose()
        {
            _btn?.Dispose();
        }

        public void Activate(Control parent, IRespondent respondent)
        {
            _curResp = respondent;
        }
        
    }
}