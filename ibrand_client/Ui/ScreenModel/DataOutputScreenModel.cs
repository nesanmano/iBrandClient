using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ScreenModel
{
    /// <summary>
    /// Cette class represente un previewScreen ou meme un DisclaimerScreen
    /// </summary>
    public class DataOutputScreenModel : AbstractScreenModel
    {

        public IDictionary<int,int[]> ContentPreviewRectangle { get; set; }

        public string ContentFileName { get;set;}

        public IDictionary<int,int[]> ContentInteractionsAreas {get;set;}

        
    }
}
