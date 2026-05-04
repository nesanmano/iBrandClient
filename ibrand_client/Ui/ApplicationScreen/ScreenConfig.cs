using System;
using System.Drawing;
using JetBrains.Annotations;
using NomadLogic.Common.Serveur;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    /// <summary>
    /// Classe englobant diverses assignations de rôles précis à des questions, et plsuieures autres informations d'intérêt global pour un écran.
    /// </summary>
    [PublicAPI]
    [Obsolete]
    public class ScreenConfig
    {
        /* Toutes les propriétés doivent être "nullable" pour déterminer si elle existent ou non avant d'extraire leur valeur.
         * Pour les primitive numériques, on doit avoir le "?" à la fin. Les référence peuvent déjà être "nullable" par définition.
         */
        public int? QuestionEmailNo { get; set; }
        public int? QuestionEmailToSendNo { get; set; }
        public int? QuestionSmsNo { get; set; }
        public int? QuestionVimeoId { get; set; }
        public int? QuestionPhotoBundleNo { get; set; }
        public int? QuestionGifNo { get; set; }
        public int? QuestionFirstPictureNo { get; set; }
        public int? QuestionPictureToPrintNo { get; set; }
        public int? QuestionPictureTransformedNo { get; set; }
        public int? QuestionPictureSocialWallNo { get; set; }
        public int? QuestionNumberOfPrintNo { get; set; }
        public int? QuestionFacebookNo { get; set; }
        public int? QuestionTwitterNo { get; set; }
        public int? QuestionVideoNo { get; set; }
        public int? QuestionCutTimeNo { get; set; }

        //signature
        public int? QuestionSignatureNo { get; set; }
        public int ProjectId { get; set; }
        public int QuestionnaireId { get; set; }

        //Kugan
        public int TerminalId { get; set; }

        [Obsolete]
        public string ProjectNo { get; set; }
        public string VideoLink { get; set; }
        
        [Obsolete]
        public int? EventId { get; set; }
        public string ProjectDescription { get; set; }

        /// <summary>
        /// Indique que l'instance ne supporte pas de chercher un fichier qui va s'avérer nul. Lance une exception si on cherche un fichier inexistant
        /// avec les méthodes "GetBitmap()" et leurs variations.
        /// </summary>
        public bool ForceFileCheck { get; set; } = true;

        public bool SendMediaToTwitter { get; set; }
        public bool ShowKeyboardOnPasswordDialog { get; set; } = true;
        public int? PotentialNumberOfPicturesTaken { get; set; }
        public Size? PreviewSize { get; set; }
        public Size? LivePreviewSize { get; set; }

        [Obsolete]
        public ScreenConfig(int projetId, int questionnaireId, int terminalId)
        {
            ProjectId = projetId;
            QuestionnaireId = questionnaireId;
            TerminalId = terminalId;
        }

        public ScreenConfig(TerminalInfo tInfo)
        {
            ProjectId = tInfo.ProjectId;
            QuestionnaireId = tInfo.QuestionnaireId;
            TerminalId = tInfo.TerminalId;
        }

        public QuestionnaireInfo GenerateQuestionnaireInfo(string questionnaireFileName, string version)
        {
            return new QuestionnaireInfo(QuestionnaireId, ProjectDescription,
                "projects\\" + ProjectId + "\\" + QuestionnaireId + "\\" + questionnaireFileName, ProjectId);
        }

        public Project GenerateProject()
        {
            return new Project(ProjectId, ProjectDescription, ProjectNo);
        }

        public IbrandEventInfo GenerateEvent()
        {
            return new IbrandEventInfo(EventId ?? -1, ProjectDescription, QuestionnaireId);
        }

        public TerminalInfo GetTerminalInfo()
        {
            return new TerminalInfo
            {
                TerminalId = TerminalId,
                QuestionnaireId = QuestionnaireId,
                ProjectId = ProjectId
            };
        }
    }
}
