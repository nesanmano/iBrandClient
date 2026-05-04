using System;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers {

    [Flags]
    public enum ShareSocialOption {
        Facebook = 2^0,
        Twitter = 2^1,
        Download = 2^2,
        Sms = 2^3,
        Email = 2^4,
        Print = 2^5
    }
}