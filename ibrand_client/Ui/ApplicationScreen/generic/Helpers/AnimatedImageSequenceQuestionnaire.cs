using System.Collections.Generic;
using System.Drawing;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// Génère une liste d'images selon une condition de questionnaire. Selon la condition, et le type de question (bundle vs image simple),
    /// peut générer de zéro à plusieures images.
    /// Ignore la valeur de langue et ne prend en compte que les réponses.
    [PublicAPI]
    public class AnimatedImageSequenceQuestionnaire : IAnimatedImageSequenceGenerator
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <inheritdoc />
        public void GenerateImages(IList<Bitmap> list, string langCode, Questionnaire quest = null, IRespondent respondent = null)
        {
            if (quest == null || Condition == null || respondent == null)
            {
                throw new System.Exception("No valid questionnaire or condition detected for image generation.");
            }

            if (!Condition.Evaluate(respondent)) return;
            var candidateFile = StaticImage ?? respondent.GetAnswer(QuestionNo, ChoiceNo).Response;

            // On assume que la réponse n'est PAS un répertoire de fichier d'un bundle.
            if (!File.Exists(candidateFile))
            {
                log.Warn($"File does not exist: {candidateFile}");
                return;
            }

            try
            {
                var image = FileUtil.LoadBitmap(candidateFile);
                if (ImagesSize != null)
                {
                    var resized = image.Resize(ImagesSize.Value.Width, ImagesSize.Value.Height);
                    image.Dispose();
                    image = resized;
                }

                list.Add(image);
            }
            catch (System.Exception e)
            {
                log.Error(e, "Could not load image from file candidate {0}", candidateFile);
            }
        }

        /// Valeur à utiliser à la place d'une réponse au questionnaire.
        public string StaticImage { get; set; }

        /// Le numéro de question.
        public int QuestionNo { get; set; }

        /// Le numéro de choix. Un par défaut.
        public int ChoiceNo { get; set; } = 1;
        
        /// Numéro de sous-choix. -1 par défaut.
        public int SubChoiceNo { get; set; } = -1;

        /// La nouvelle taille des images voulues. La taille des images originales des questions sont probablement
        /// inapropriées.
        public Size? ImagesSize { get; set; }

        /// Valeur maximale de la séquence.
        public ICondition Condition { get; set; }

    }
}