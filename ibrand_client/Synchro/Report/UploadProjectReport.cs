namespace NomadLogic.iBrandDesktop.Synchro.Report
{
    public class UploadProjectReport : TcpCommunicationProgressReport
    {
        /// <summary>
        /// Indique le nombre de répondants qui seront traités.
        /// </summary>
        public int TotalRespondentToSend { get; set; }

        /// <summary>
        /// Indique que le répondant (avec identifiant local) à terminé d'être téléversé vers le serveur.
        /// </summary>
        public int NbrRespondentsUploaded { get; set; }

        /// <summary>
        /// Indique le total de bytes envoyés jusqu'à maintenant.
        /// </summary>
        public ulong TotalBytesSent { get; set; }

        /// <summary>
        /// Indique le total approximatif de bytes à envoyer.
        /// </summary>
        public ulong TotalProjectedBytesSent { get; set; }

        /// <summary>
        /// Indique que l'envoi est terminé pour ce projet.
        /// </summary>
        public bool UploadFinished { get; set; }

        /// <summary>
        /// Si cette valeur est non-null, il faut indiquer que ce questionnaire est invalide et que plus aucune tentative de synchronisation ne doit être tentée.
        /// Cet état est non-récupérable à moins de modifier les infos sur disque du questionnaire.
        /// </summary>
        public QuestionnaireInfo InvalidateQuestionnaire { get; set; }
    }
}