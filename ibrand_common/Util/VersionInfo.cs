namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Structure standard pour indiquer un numéro de version de logiciel.
    /// Basé sur la convention de séquence incrémentale (https://en.wikipedia.org/wiki/Software_versioning)
    /// </summary>
    public class VersionInfo
    {
        public int Major { get; set; }
        public int Minor { get; set; } = -1;
        public int Debug { get; set; } = -1;
        public int Build { get; set; }

        public bool CheckVersion(params string[] strings)
        {
            var nbrParams = strings.Length;

            if (nbrParams == 0) return false;
            if (nbrParams >= 1)
            {
                if (!int.TryParse(strings[0], out var m)) return false;
                if (m < Major) return false;
            }
            if (nbrParams >= 2)
            {
                if (!int.TryParse(strings[1], out var m)) return false;
                if (m < Minor) return false;
            }
            if (nbrParams >= 3)
            {
                if (!int.TryParse(strings[2], out var m)) return false;
                if (m < Debug) return false;
            }
            if (nbrParams >= 4)
            {
                if (!int.TryParse(strings[3], out var m)) return false;
                if (m < Build) return false;
            }
            return true;
        }

        public bool CheckVersion(params int[] numbers)
        {
            var nbrParams = numbers.Length;

            if (nbrParams == 0) return false;
            if (nbrParams >= 1)
            {
                if (numbers[0] < Major) return false;
            }
            if (nbrParams >= 2)
            {
                if (numbers[1] < Minor) return false;
            }
            if (nbrParams >= 3)
            {
                if (numbers[2] < Debug) return false;
            }
            if (nbrParams >= 4)
            {
                if (numbers[3] < Build) return false;
            }
            return true;
        }
    }
}