using System.Collections.Generic;
using System.Drawing;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction
{
    /// <summary>
    /// Classe de transformation de photo. Requiert un fichier de transformation appellé "picture_transformation_config.xml" dans le répertoire
    /// du questionnaire.
    /// </summary>
    public class FoodNetworkBaconFestSubmitAction : AbstractSubmitActionPictureTransformation
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("Transformation", 1);
        }

        public string DestinationFolder => _configData.ContainsKey("destination_folder") ? _configData["destination_folder"] : null;

        public override bool SubmitRespondent(Respondent respondent)
        {
            try
            {
                var db = ServicesContainer.GetService<IbrandDatabase>();


                var respId = db.InsertRespondent(respondent, false);

                var temp = new PictureTransformSubmitActionWorker(this, respondent, respId);
                _submitActionWorkerPools[0].AddWorker(temp);

                RespondentSaved();

                return true;
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex.Message);
                return false;
            }
        }

        public override IList<Bitmap> TransformPictures(IRespondent resp, bool preview)
        {
            return PictureTransformationController.TransformPictures(resp, preview);
        }

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new PictureTransformSubmitActionWorker(this, resp, resp.Id);
        }
    }
}
