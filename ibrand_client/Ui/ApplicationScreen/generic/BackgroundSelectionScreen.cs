using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /// <summary>
    /// Écran de sélection générique avec différentes zones.
    /// </summary>
    [PublicAPI]
    [Obsolete]
    public class BackgroundSelectionScreen : AbstractScreen
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public IList<AnswerZone> BackgroundSelectionOptions { get; set; } = new List<AnswerZone>();

        public string BackgroundSelectionOptionsConfigFile { get; set; }

        protected AnswerZone _selectedOption;

        /// Si les zones ne sont pas automatiques, détermine si on peut passer au prochain écran.
        [PublicAPI] 
        public virtual bool HasAnswer => _selectedOption?.AnswersUponSelection != null && _selectedOption?.AnswersUponSelection?.Count > 0;

        public Rectangle? NextArea { get; set; }

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        /// <param name="backgroundImgEnSrc">Fond anglais</param>
        /// <param name="backgroundImgFrSrc">Fond français</param>
        /// <param name="sc">Configuration</param>
        public BackgroundSelectionScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, ScreenConfig sc)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            var basePath = DirectoryUtil.GetLocalIbrandQuestionnaireDir(_screenConfig.ProjectId, _screenConfig.QuestionnaireId);

            if (!string.IsNullOrWhiteSpace(BackgroundSelectionOptionsConfigFile))
            {
                var configFile = basePath + BackgroundSelectionOptionsConfigFile;

                if (!File.Exists(configFile)) return;
                var obj = JsonUtil.ReadJsonFile<List<AnswerZone>>(new FileInfo(configFile));
                BackgroundSelectionOptions = obj ?? throw new System.Exception("Could not create instance of background options with file: " + configFile);
            }

            //TODO a deplacer dans une methode qui regait sur un changement de langue
            foreach (var option in BackgroundSelectionOptions)
            {
                if (!string.IsNullOrWhiteSpace(option.BackgroundImageFilePath) && File.Exists(basePath + option.BackgroundImageFilePath))
                {
                    var bmpFile = basePath + option.BackgroundImageFilePath;
                    option.BackgroundImageInstance =  FileUtil.LoadBitmap(bmpFile);
                }
            }
        }
        
        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();
            _selectedOption = null; 
        }

        protected override void OnPaintEvent(Graphics g) {
            base.OnPaintEvent(g);

            foreach (var option in BackgroundSelectionOptions)
            {
                DrawImage(g, option.BackgroundImageInstance, option.Zone);
            }

            if (_selectedOption?.SelectionActivatedImage == null) return;
            var p = _selectedOption.SelectionActivatedImageLocation ?? _selectedOption.Zone.Location;
            DrawImage(g, _selectedOption.SelectionActivatedImage, p);
        }

        /// Message à l'intention de l'usager pour non-sélection. Version française.
        [PublicAPI]
        public string MessageNoSelectionFr { get; set; } = Resources.iBrand.NoBackgroundSelected; // TODO: Ce qui est une erreur. Faudrait avoir un meilleur système i18n
        
        /// Message à l'intention de l'usager pour non-sélection. Version anglaise.
        [PublicAPI]
        public string MessageNoSelectionEn { get; set; } = Resources.iBrand.NoBackgroundSelected; // TODO: Ce qui est une erreur. Faudrait avoir un meilleur système i18n
        
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, NextArea))
            {
                if (!HasAnswer)
                {
                    var str = _activeLg.StartsWith("fr", StringComparison.InvariantCultureIgnoreCase) ?
                        MessageNoSelectionFr :
                        MessageNoSelectionEn;

                    var mBox = new MessageDialog();
                    mBox.ShowMessage(str);
                    return false;
                }

                SaveSelectedOptionToQuestionnaire();

                var opt = _selectedOption?.PageChangeOptions?.Evaluate(GetCurrentRespondentWithAnswers());
                if (opt == null)
                {
                    Next(1);
                    return false;
                }

                this.ExecuteScreenChangeOption(opt);
            }
            else
            {
                foreach (var z in BackgroundSelectionOptions.Where(z => InArea(x, y, z.Zone)))
                {
                    var sco = z.PageChangeOptions?.Evaluate(GetCurrentRespondentWithAnswers());

                    _selectedOption = z;
                        
                    if (sco != null)
                    {
                        if (NextArea == null)
                        {
                            SaveSelectedOptionToQuestionnaire();

                            this.ExecuteScreenChangeOption(sco);

                            return false;
                        }
                    }

                    Refresh();
                }
            }

            return true;
        }

        protected void SaveSelectedOptionToQuestionnaire()
        {
            if (_selectedOption?.AnswersUponSelection == null || !_selectedOption.AnswersUponSelection.Any()) return;
            foreach (var a in _selectedOption.AnswersUponSelection)
            {
                SaveAnswerToQuestion(a);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var option in BackgroundSelectionOptions)
            {
                option.Dispose();
            }
        }
    }
}
