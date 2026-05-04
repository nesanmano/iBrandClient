using System;
using System.Drawing;
using Newtonsoft.Json;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Contient les informations et les méthodes d'aide pour afficher du contenu du questionnaire dans un "Label" de Windows From.
    /// </summary>
    public class QuestionnaireAnswerImage : IDisposable, IImageInitialisable
    {

        /// Le point d'ancre pour dessiner l'image.
        public Point ImagePoint;

        /// Référence vers l'instance de l'image, une fois initialisée.
        [JsonIgnore]
        public Bitmap ImageInstance { get; private set; }

        /// Indique si le dernier check de condition a été vrai. Variable d'état à accès rapide.
        [JsonIgnore]
        public bool ConditionCheck { get; set; }

        /// La condition qui détermine si on affiche l'image ou non.
        public ICondition Condition { get; set; }

        /// Le générateur d'image.
        public IImageGenerator ImageGenerator { get; set; }

        /// Met à jour l'instance selon le répondant donné. On peut calculer cela manuellement via la condition.
        public void Update(IRespondent resp)
        {
            ConditionCheck = Condition?.Evaluate(resp) ?? false;
        }

        public bool InitImage(string baseDir, string languageCode)
        {
            ImageInstance?.Dispose();
            ImageInstance = ImageGenerator.GenerateImage(languageCode);

            return ImageInstance != null;
        }

        public void Dispose()
        {
            ImageInstance?.Dispose();
        }

        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}