using System;
using System.Collections.Generic;
using System.Drawing;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Util.SubmitAction.BaseClasses;
using NomadLogic.iBrandDesktop.Util.SubmitAction.Generic.Worker;

namespace NomadLogic.iBrandDesktop.Util.SubmitAction.Generic
{
    /// <summary>
    /// Classe de transformation de photo. Toutes les configurations se trouvent dans le fichier "config.txt" contenu dans la
    /// racine du projet, et non questionnaire.
    /// Il "fake" l'utilisation d'un fichier de transformation fichier.
    /// </summary>
    public class GifTransformationSubmitAction : AbstractSubmitActionPictureTransformation
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected override void InstanciateSubmitActionPools()
        {
            AddSubmitActionWorkerPool("Gif_Transformation", 1);
        }

        /// <inheritdoc />
        protected override void AdditionalInitOperations()
        {
            // rieng, on fake un transformationController
        }

        /// <inheritdoc />
        public override IList<Bitmap> TransformPictures(IRespondent resp, bool preview)
        {
            throw new NotImplementedException();
        }
        
        #region configs

        public string NoQuestionPhotoBundle
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("no_question_photo_bundle"))
                {
                    return _configData["no_question_photo_bundle"];
                }

                return null;
            }
        }

        public string NoQuestionFirstPicture
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("no_question_first_picture"))
                {
                    return _configData["no_question_first_picture"];
                }

                return null;
            }
        }

        public string NoQuestionGif
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("no_question_gif"))
                {
                    return _configData["no_question_gif"];
                }

                return null;
            }
        }

        public string Overlay
        {
            get
            {
                if (_configData != null && _configData.ContainsKey("overlay"))
                {
                    return _configData["overlay"];
                }

                return null;
            }
        }

        public Size GifSize
        {
            get
            {
                if (_configData == null || !_configData.ContainsKey("size")) return new Size(500, 500);
                return StringUtil.SizeFromStringDefinition(_configData["size"]);
            }
        }

        public int Delay
        {
            get
            {
                if (_configData == null || !_configData.ContainsKey("delay")) return 300;
                int.TryParse(_configData["delay"], out int val);
                return val;
            }
        }

        public int NbrIteration
        {
            get
            {
                if (_configData == null || !_configData.ContainsKey("iteration")) return 1;
                int.TryParse(_configData["iteration"], out int val);
                return val;
            }
        }

        public bool RewindGif
        {
            get
            {
                if (_configData == null || !_configData.ContainsKey("rewind")) return false;
                bool.TryParse(_configData["rewind"], out bool val);
                return val;
            }
        }

        public bool Normalize
        {
            get
            {
                if (_configData == null || !_configData.ContainsKey("normalize_pictures")) return false;
                bool.TryParse(_configData["normalize_pictures"], out bool val);
                return val;
            }
        }

        public Size NormalizeSize
        {
            get
            {
                if (_configData == null || !_configData.ContainsKey("normalize_crop_size")) return new Size(500, 500);
                return StringUtil.SizeFromStringDefinition(_configData["normalize_crop_size"]);
            }
        }

        #endregion

        protected override ISubmitActionWorker CreateWorker(Respondent resp, bool usePool = true)
        {
            return new GifTransformationSubmitActionWorker(this, resp.Id);
        }
    }
}
