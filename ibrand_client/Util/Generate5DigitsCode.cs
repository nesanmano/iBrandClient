using System;

namespace NomadLogic.iBrandDesktop.Util
{
    public class Generate5DigitsCode
    {
        public static string GetCode()
        {
            var random = new Random();
            return Convert.ToString(random.Next(100000, 999999)); 
        }
    }
}
