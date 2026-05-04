using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.iBrandDesktop.Services.RespondentEventDataDownload
{
    /// <summary>
    /// Représente une demande de téléchargement. L'égalité se fait sur une seule valeur: l'identifiant de répondant.
    /// </summary>
    [PublicAPI]
    public class RespondentBundleRequest : IEquatable<RespondentBundleRequest>, IComparable<RespondentBundleRequest>
    {
        private readonly DirectoryInfo _destinationBase;
        private readonly string _respondentUniqueId;

        /// <summary>
        /// Nombre d'essais complétés (avec succès ou non) sur ce fichier.
        /// Cette valeur est transitive et ne fait pas partie de la vérification d'égalité.
        /// </summary>
        internal int NbrTries { get; set; }
        
        public Action<RespondentBundleRequest> CallbackError { get; }

        public Action<RespondentBundleRequest> CallbackComplete { get; }

        public Action<RespondentBundleRequest> CallbackCancelled { get; }

        public IRespondentEventDataDownloadWorker RespondentDataWorkerInstance { get; }

        public ISaveLocalRespondentData GetSaveObject { get; }

        /// <summary>
        /// Constructeur principal d'une requête de téléchargement via identifiant externe unique.
        /// </summary>
        /// <param name="baseDirectory">Le répertoire de base où sauvegarder les objets de la couche de persistance.</param>
        /// <param name="respUniqueId">La valeur unique pour identifier le répondant source sur le serveur.</param>
        /// <param name="worker">L'instance de l'objet qui va traiter le résultat de la demande vers le serveur.</param>
        /// <param name="dataSaveObject">L'instance où sauvegarder les informations directement dans le questionnaire actif.</param>
        /// <param name="callbackSuccess">Méthode d'Appel en cas de succès. Optionel.</param>
        /// <param name="callbackCancelled">Méthode d'Appel en cas d'annulation. Optionel.</param>
        /// <param name="callbackError">Méthode d'Appel en cas d'erreur. Optionel.</param>
        public RespondentBundleRequest([NotNull] DirectoryInfo baseDirectory, [NotNull] string respUniqueId, [NotNull] IRespondentEventDataDownloadWorker worker,
            [NotNull] ISaveLocalRespondentData dataSaveObject,
            Action<RespondentBundleRequest> callbackSuccess = null, 
            Action<RespondentBundleRequest> callbackCancelled = null, 
            Action<RespondentBundleRequest> callbackError = null)
        {
            _destinationBase = baseDirectory;
            if (string.IsNullOrWhiteSpace(respUniqueId))
            {
                throw new ArgumentException("RespondentId cannot be empty or null.");
            }

            RespondentDataWorkerInstance = worker;
            _respondentUniqueId = respUniqueId;

            GetSaveObject = dataSaveObject;

            CallbackComplete = callbackSuccess;
            CallbackError = callbackError;
            CallbackCancelled = callbackCancelled;
        }

        public DirectoryInfo GetBaseDirectory()
        {
            return _destinationBase;
        }

        public string GetRespondentUniqueId()
        {
            return _respondentUniqueId;
        }
        
        public bool Equals(RespondentBundleRequest other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_respondentUniqueId, other._respondentUniqueId) && _destinationBase == other._destinationBase;
        }

        public override bool Equals(object obj)
        {
            if (obj is RespondentBundleRequest other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_destinationBase != null ? _destinationBase.GetHashCode() : 0) * 397) ^ (_respondentUniqueId != null ? _respondentUniqueId.GetHashCode() : 0);
            }
        }

        public static bool operator ==(RespondentBundleRequest left, RespondentBundleRequest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RespondentBundleRequest left, RespondentBundleRequest right)
        {
            return !Equals(left, right);
        }

        public int CompareTo(RespondentBundleRequest other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            var urlComparison = string.Compare(_respondentUniqueId, other._respondentUniqueId, StringComparison.Ordinal);
            if (urlComparison != 0) return urlComparison;
            return string.Compare(_destinationBase.FullName, other._destinationBase.FullName, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append('[');
            sb.Append("UniqueId='");
            sb.Append(_respondentUniqueId);
            sb.Append("', dest='");
            sb.Append(_destinationBase.FullName);
            sb.Append("']");
            return sb.ToString();
        }

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(_respondentUniqueId) && RespondentDataWorkerInstance.Validate();
        }
    }
}