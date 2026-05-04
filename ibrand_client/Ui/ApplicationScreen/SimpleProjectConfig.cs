using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen
{
    public class SimpleProjectConfig
    {
        public IDictionary<string,int[]> HomeScreen { get; set; }

        public IDictionary<string,int[]> Preview { get; set; }

        public IDictionary<string,int[]> Questions { get; set; }

        public IDictionary<string, int[]> PrintScreen { get; set; }


        public int[] TakePicture { get; set; }

        public bool Print { get; set; }

        public bool SkipQuestionnaire { get; set; }

        public int[] Disclaimer { get; set; }

        public int PrintLimit { set; get; } = -1;
    }
}
