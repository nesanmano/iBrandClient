using System;
using System.Drawing;
using System.Linq;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.iBrandDesktop.Exception;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.Multimedia.XmlTransformation.image;
using FileNotFoundException = System.IO.FileNotFoundException;

namespace NomadLogic.iBrandDesktop.Util
{
    [Obsolete("Utiliser une autre façon qu'un singleton pour utiliser l'instance de transformation. Il faudra probablement refactoriser l'instance de tarnsformation.")]
    public class PictureController
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static PictureController _instance;

        private Bitmap _transformedImage;
        private bool _questionsValidated;
        private RespondentImpl _mockRespondent;
        private bool _pictureTaken;

        private PictureController()
        {

        }

        public static PictureController GetInstance()
        {
            return _instance ?? (_instance = new PictureController());
        }
        
        public void OnAnswerChanged()
        {
            PictureTransformed = false;
            _questionsValidated = false;
        }

        public Bitmap GetTransformedPicture()
        {
            if (_pictureTaken)
            {
                if (!PictureTransformed)
                {
                    TransformPicture();
                }
            }
            else
            {
                throw new FileNotFoundException();
            }

            return _transformedImage;
        }

        private void TransformPicture()
        {
            var questionnaire = QuestionnaireControllerAccessor.QuestionnaireController.Questionnaire;
            var submitAction = questionnaire.SubmitAction as AbstractSubmitActionPictureTransformation;
            if (submitAction == null) return;

            var refaireResp = false;
            var controller = new TransformationsController();
            controller.Initialize(ProjectAccessor.Project.ProjectDir + submitAction.PictureTransformationConfigFile);
            // Valider qu'on a bien toutes les réponses pour la transformation
            if (!_questionsValidated)
            {
                _questionsValidated = true;
                foreach (
                    var question in
                        controller.GetQuestionDependency()
                            .Select(questionNo => questionnaire.GetQuestion(questionNo)))
                {
                    if (!question.ValidateAnswers())
                    {
                        QuestionnaireControllerAccessor.QuestionnaireController.ShowMessageBox(
                            Resources.iBrand.MandatoryQuestion);
                        _questionsValidated = false;
                    }

                    try
                    {
                        if (!question.ValidateAnswersPattern())
                        {
                            QuestionnaireControllerAccessor.QuestionnaireController.ShowMessageBox(
                                Resources.iBrand.WrongPattern1 + " " + question.No + " " +
                                Resources.iBrand.WrongPattern2 + question.AnswersPattern);
                            _questionsValidated = false;
                        }
                    }
                    catch (UnsuportedPatternException)
                    {
                        //TBD
                    }

                    try
                    {
                        if (!question.ValidateDatasourceField())
                        {
                            QuestionnaireControllerAccessor.QuestionnaireController.ShowMessageBox(
                                Resources.iBrand.ValueNotInDataSource + " " + question.No);
                            _questionsValidated = false;
                        }
                    }
                    catch (System.Exception)
                    {
                        //TBD
                    }
                }

                refaireResp = true;
            }

            if (!_questionsValidated) return;

            if (_mockRespondent == null || refaireResp)
            {
                _mockRespondent = new RespondentImpl(questionnaire.GetRespondentLanguage(),
                    ProjectAccessor.Project.ProjectDir,
                    QuestionnaireControllerAccessor.QuestionnaireController.IbrandEventInfo.Id);
                /*
                foreach (var reponse in questionnaire.GetAnswersToSaveInDb())
                {
                    string reponseText = null;
                    // Si la réponse est une réponse combo, on veux le sous-choix
                    if (reponse.QuestionType == 3)
                    {
                        reponseText = reponse.SubChoiceNo;
                    }
                    // s'il s'agit d'une question radio (type == 2) ou une question checkbox (type == 1), il n'y a pas de texte dans la réponse.
                    else if (reponse.QuestionType != 2 && reponse.QuestionType != 1)
                    {
                        reponseText = reponse.Response;
                    }
                    _mockRespondent.AddAnswer(reponse.QuestionNo, reponse.ChoiceNo, reponseText);
                }
                */
            }
                
            _transformedImage?.Dispose();
            _transformedImage = controller.TransformPictures(_mockRespondent, false)[0];
            PictureTransformed = true;
        }

        public Bitmap TransformedPicture
        {
            get { return _transformedImage; }
            set
            {
                _transformedImage = value;
                PictureTransformed = _transformedImage != null;
            }
        }

        public bool PictureTransformed { get; private set; }

        /*public void Reset()
        {
            _pictureTaken = false;
            if (CaptureWidget != null)
            {
                CaptureWidget.PictureTaken = false;
            }
            PictureTransformed = false;
            _questionsValidated = false;
            _mockRespondent = null;
            _originalFile = null;
            _transformedImage?.Dispose();
            _transformedImage = null;
        }*/

        public void Dispose()
        {
            _transformedImage?.Dispose();
            _transformedImage = null;
        }

        //public CaptureTextBoxNikon CaptureWidget { Get; Set; }
    }
}
