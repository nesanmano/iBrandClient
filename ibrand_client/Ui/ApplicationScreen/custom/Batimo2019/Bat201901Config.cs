using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom.Batimo2019
{
    public class Bat201901Config
    {
        public bool UseLanguageSelection { get; set; }
        public bool UseBackgroundSelection { get; set; }
        public string BackGroundSelectionConfigFile { get; set; }
        public Rectangle? PreviewLocation { get; set; }
        public Point CountDownLocation { get; set; }
        public string TransformationOptionsFile { get; set; }
        public Rectangle TransformPreviewLocation { get; set; }
        public bool UseQuestionnaireScreen { get; set; }
    }
}