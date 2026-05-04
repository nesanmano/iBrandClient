using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ScreenModel
{
    public class CameraInputScreenModel: AbstractScreenModel
    {

        public IList<string> CountImageSequenceFiles {get;set;}

        public IDictionary<string,SaveAnswer> OverlaySelector {get;set;}

        public int[] CountDownLocation {get;set;}

        public int[] OverlayPosition { get;set;}

        public IDictionary<string,int[]> TransformationOptions {get;set;}

        public int MaxCountDown {get;set;}

        public int[] PreviewRectangle {get;set;}





    }
}
