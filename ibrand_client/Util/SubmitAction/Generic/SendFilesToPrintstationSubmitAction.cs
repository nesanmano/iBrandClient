using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    /// <summary>
    /// Cette action générique envoie un ou plusieurs fichiers tirées des questions d'un questionnaire, vers le ou les Printstations
    /// via le port de communication direct.
    /// <br/>Il faut spécifier dans le fichier de configuration par défaut d'un AbstractSubmitAction (config.txt dans le répertoire du questionnaire)
    /// quels sont les questions à envoyer, et combien de Printstations minimum il faut avoir envoyé les fichiers pour considérer l'opération
    /// comme un succès.
    /// </summary>
    public class SendFilesToPrintstationSubmitAction : AbstractSubmitActionSendFilesToPrintstation
    {
        /// <inheritdoc/>
        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new SendFilesToPrintstationWorker(this, resp, resp.Id);
        }
    }
    
}
