using System.Collections.Generic;

namespace NomadLogic.iBrandDesktop.Ui.ScreenModel
{
    public abstract class AbstractScreenModel : IComparer<AbstractScreenModel>
    {

        //bool IsDisclaimer { get;set;}

        public IDictionary<string,string> BackgroundImages { get; set;}

        public string Clazz { get;set;}

        public IDictionary<string,int[]> ScreenInteractionFeatures { get;set;}

        public IDictionary<int,int[]> InformationAndDisclaimers { get;set;}

        public bool Skip { get;set;}

        public int Order { get;set;}

        public int RequiredJumpsToNextScreen {get;set;}

        public int RequiredJumpsToPreviousScreen {get;set;}

        public SaveAnswer SaveToQuestion { get; set; }  // uniquement pour des screen simple pas applicable pour les questionnaire complexe avec plusiwurs question

        public IDictionary<int,InputModel> CheckBoxes { get; set; }// Opt-ins

        public int InactivityInterval = 3000;

        public int Compare(AbstractScreenModel x, AbstractScreenModel y)
        {

            if (x.Order < y.Order) return 1;
            else if (x.Order == y.Order) return 0;
            else return -1; // means y is greater than x
        }
    }
}
