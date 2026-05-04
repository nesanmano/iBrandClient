using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

namespace NomadLogic.PrintStation.Ui.generic
{
    /// <summary>
    /// Cet écran représente un branchement. On peut lui donner plusieures zones (dont une par défaut si on ne fait aucune zone manuellement),
    /// où chaque zone représente un saut de page plus loin, de façon à sauter des sections complètes du questionnaires.
    /// Une zone de cancellation (optionnelle) revient à l'écran principal, signifiant une annulation.
    /// Ne pas oublier que donner une page supérieure au nombre de page du questionnaire indique que l'on doit terminer le questionnaire et
    /// sauvegarder les données (contrairement à l'annulation, qui n'enregistre rien).
    /// Si on n'a pas de zones et un rectangle de "Cancel", cet écran devient un "dead-end" volontaire pour annuler un répondant sans chance
    /// de revenir en arrière, ce qui est parfois le but voulu.
    /// </summary>
    [PublicAPI]
    public class SectionSkipScreen : AbstractPrintStationScreen
    {
        private readonly Rectangle? _cancelZone;
        private SectionSkipScreenData[] _zones;

        private  Bitmap _checkmark;
        public  IList<ScreenCheckOption> CheckBoxZones { get; set; }


        /// Chemin de fichier complet pour les règles en anglais.
        public string RuleAndRegsFileEn { get; set; }
        /// Chemin de fichier complet en français
        public string RuleAndRegsFilefr { get; set; }

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly TextBox _rulesRegsTextbox;

        protected virtual void OnLoad(object sender, EventArgs e)
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");

        }

        protected override void OnLoad()
        {
            _checkmark = GetNoneLanguageBitmap("checkmark.png");
            UpdateRulesAndRegs();
        }

        /// <summary>
        /// Assigne ou va chercher un tableau de définition de zones.
        /// Cette valeur ne peut jamais être vide ou nulle, et dans cette situation on lui assignera une valeur 
        /// par défaut (Tout l'écran, va à la prochaine page) la prochaine fois qu'on demandera sa valeur.
        /// </summary>
        public SectionSkipScreenData[] Zones
        {
            get => _zones ?? (_zones = new[]
            {
                new SectionSkipScreenData
                {
                    PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(),
                    Zone = new Rectangle(new Point(0, 0), new Size(PictureBox.Image.Width, PictureBox.Image.Height))
                }
            });

            set
            {
                if (value == null || value.Length == 0)
                {
                    _zones = null;
                }
                else
                {
                    _zones = value;
                }
                
            }
        }

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        /// <param name="backgroundImgEnSrc">Fond d'écran anglais.</param>
        /// <param name="backgroundImgFrSrc">Fond d'écran français.</param>
        /// <param name="sc">Paramètre de configuration généraux de l'écran.</param>
        /// <param name="cancelZone">Optionnel. Zone de cancellation (retour à l'écran 1 sans sauvegarde).</param>
        /// <param name="rulesAndRegsZone">Optionnel. Zone où faire apparaître les règles. Nécessite que que les chemin vers les fichiers sont initialisés.</param>
        public SectionSkipScreen(string backgroundImgEnSrc, string backgroundImgFrSrc, PrintStationScreenConfig sc, Rectangle? cancelZone = null, Rectangle? rulesAndRegsZone = null)
            : base(backgroundImgEnSrc, backgroundImgFrSrc, sc)
        {
            if (rulesAndRegsZone != null)
            {
                _rulesRegsTextbox = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    WordWrap = true,
                    Location = rulesAndRegsZone.Value.Location,
                    Size = rulesAndRegsZone.Value.Size,
                    Enabled = true,
                    Visible = true,
                    ReadOnly = true,
                    TextAlign = HorizontalAlignment.Left

                };

                Controls.Add(_rulesRegsTextbox);
                _rulesRegsTextbox.BringToFront();
            }
            _cancelZone = cancelZone;
        }

        private void UpdateRulesAndRegs()
        {
             if (_rulesRegsTextbox == null) return;
            this.SafeCall(() => {
                var file = _activeLg.Equals("fr", StringComparison.InvariantCultureIgnoreCase) ? RuleAndRegsFilefr : RuleAndRegsFileEn;
                if (!File.Exists(file))
                {
                    log.Warn($"No file for for text ({_activeLg}): {file}");
                    return;
                }
                _rulesRegsTextbox.Text = File.ReadAllText(file);
            });
        }

        public override bool ChangeLanguage(string language)
        {
            var change = base.ChangeLanguage(language);
            if (!change) return false;
            UpdateRulesAndRegs();

            return true;
        }

        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, _cancelZone))
            {
                Close();
                return false;
            }

            if (!EvaluateCheckBoxsMouseDown(x, y, CheckBoxZones)) return false;

            return this.EvaluateZonesAndValidation(x, y, Zones, CheckBoxZones);
        }

        /// <inheritdoc/>
        public override void Recycle()
        {
            base.Recycle();
            RecycleCheckBoxs(CheckBoxZones);
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            PaintCheckBoxList(g, CheckBoxZones, _checkmark);
        }

        public override void Dispose()
        {
            base.Dispose();

            _checkmark?.Dispose();
            _checkmark = null;
        }
    }
}