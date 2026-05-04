using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ScreenModel
{
    public class DataInputScreenModel : AbstractScreenModel
    {
        public IDictionary<int,InputModel>  TextBoxes {get;set; }

        public int[] Keyboard {get;set;}
    }
}
