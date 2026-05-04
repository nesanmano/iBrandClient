namespace NomadLogic.Common
{
    public enum QuestionTypeEnum
    {
        Check = 1,
        Radio = 2,
        Combo = 3,
        Text = 4,
        Photo = 5,
        Email = 6,
        Video = 7,
        Signature = 8,
        Audio = 9,
        Background = 10,
        PhotoText = 11,
        PhotoBundle = 12,
        Merge = 13,
        Vimeo = 14,
        Sms = 15,
        //Dynamic = 16, // Complètement déprécié.
        ExternalUniqueIdentifier = 17
    }

    public static class QuestionTypeExtension
    {
        public static bool IsFileType(this QuestionTypeEnum qe)
        {
            if (qe == QuestionTypeEnum.Background || 
                qe == QuestionTypeEnum.Photo || 
                qe == QuestionTypeEnum.PhotoBundle || 
                qe == QuestionTypeEnum.PhotoText || 
                qe == QuestionTypeEnum.Audio || 
                qe == QuestionTypeEnum.Signature ||
                qe == QuestionTypeEnum.Video)
            {
                return true;
            }

            return false;
        }
    }
}