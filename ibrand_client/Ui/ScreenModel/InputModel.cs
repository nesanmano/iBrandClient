

using System.Collections.Generic;
using System.Drawing;

namespace NomadLogic.iBrandDesktop.Ui.ScreenModel
{
    public class InputModel
    {
        public int[] InputArea {get;set;}

        public bool IsCheckbox {get;set;}

        public int[] CheckAnchorPoint {get;set;}

        public string Desc {get;set;}

        public IDictionary<string,string> ErrorMessages {get;set;} 

        public SaveAnswer SaveToQuestion {get;set;}

        public Rectangle getInputRectangle()
        {
            return new Rectangle(InputArea[0],InputArea[1],InputArea[2],InputArea[3]);
        }


    }
}
