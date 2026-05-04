using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Respondent;

namespace NomadLogic.Common.Xangle
{
    public class XangleIbrandMapping
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [JsonConverter(typeof(ConditionJsonConverter))]
        public ICondition Condition { get; set; }

        [JsonRequired]
        public int QuestionNo { get; set; }

        public int ChoiceNo { get; set; } = 1;

        public int SubChoiceNo { get; set; } = -1;

        [JsonConverter(typeof(StringEnumConverter)), JsonRequired]
        public XangleItemType ItemType { get; set; }

        public int ThumbIndex { get; set; } = 0;

        /// <summary>
        /// Indique s'il y a une correspondance entre le répondant et l'élément de mapping présent.
        /// </summary>
        /// <param name="resp">Le répondant à analyser.</param>
        /// <param name="xangleItem">L'objet Xangle à analyser.</param>
        /// <param name="answer">Out. La réponse à mettre dans la couche de persistance.</param>
        /// <param name="isCheck">Indique si la réponse est un "checkbox".</param>
        /// <returns>TRUE si et seulement si une correspondance existe. Si faux, les paramètre "out" sont vides.</returns>
        public bool FindMapping(IRespondent resp, XangleItem xangleItem, out Answer answer, out bool isCheck)
        {
            answer = null;
            isCheck = false;
            if (!Condition?.Evaluate(resp) ?? false) return false; // Vérification de la condition.

            switch (ItemType)
            {
                case XangleItemType.Video:
                    var vid = xangleItem.GetVideoFile();
                    if (vid == null) return false;
                    if (!vid.Exists)
                    {
                        log.Warn("XangleItem returns video file, but file does not exists: {0}", vid);
                        return false;
                    }

                    answer = new Answer //(7, QuestionNo, ChoiceNo, SubChoiceNo, vid.FullName);
                    {
                        QuestionType = QuestionTypeEnum.Video,
                        QuestionNo = QuestionNo,
                        ChoiceNo = ChoiceNo,
                        Response = vid.FullName
                    };
                    break;
                case XangleItemType.Gif:
                    var gif = xangleItem.GetGifFile();
                    if (gif == null) return false;
                    if (!gif.Exists)
                    {
                        log.Warn("XangleItem returns gif file, but file does not exists: {0}", gif);
                        return false;
                    }
                    answer = new Answer //(11, QuestionNo, ChoiceNo, SubChoiceNo, gif.FullName);
                    {
                        QuestionType = QuestionTypeEnum.PhotoText,
                        QuestionNo = QuestionNo,
                        ChoiceNo = ChoiceNo,
                        Response = gif.FullName
                    };
                    break;
                case XangleItemType.Thumbnail:
                    var thumb = xangleItem.GetThumbnails();
                    if (thumb == null || thumb.Length <= ThumbIndex) return false;
                    var thumbFile = thumb[ThumbIndex];
                    if (!thumbFile.Exists)
                    {
                        log.Warn("XangleItem returns image file, but file does not exists: {0}", thumbFile);
                        return false;
                    }
                    answer = new Answer //(11, QuestionNo, ChoiceNo, SubChoiceNo, thumbFile.FullName);
                    {
                        QuestionType = QuestionTypeEnum.PhotoText,
                        QuestionNo = QuestionNo,
                        ChoiceNo = ChoiceNo,
                        Response = thumbFile.FullName
                    };
                    break;
                case XangleItemType.ThumbnailSmall:
                    var small = xangleItem.GetSmallThumbnail();
                    if (small == null) return false;
                    if (!small.Exists)
                    {
                        log.Warn("XangleItem returns image file, but file does not exists: {0}", small);
                        return false;
                    }
                    answer = new Answer //(11, QuestionNo, ChoiceNo, SubChoiceNo, small.FullName);
                    {
                        QuestionType = QuestionTypeEnum.PhotoText,
                        QuestionNo = QuestionNo,
                        ChoiceNo = ChoiceNo,
                        Response = small.FullName
                    };
                    break;
                case XangleItemType.Bundle:
                    var normalDir = xangleItem.GetNormalizedImagesDirectory();
                    if (normalDir == null) return false;
                    if (!normalDir.Exists)
                    {
                        log.Warn("XangleItem returns aligned images directory, but it does not exists: {0}", normalDir);
                        return false;
                    }
                    answer = new Answer //(11, QuestionNo, ChoiceNo, SubChoiceNo, normalDir.FullName);
                    {
                        QuestionType = QuestionTypeEnum.PhotoText,
                        QuestionNo = QuestionNo,
                        ChoiceNo = ChoiceNo,
                        Response = normalDir.FullName
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }
    }
}