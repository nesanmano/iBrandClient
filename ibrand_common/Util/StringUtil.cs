using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Math;

// ReSharper disable InconsistentNaming

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Contient des méthodes utiles pour le traitement de chaînes de caractères pour la suite d'application de Nomad Logic
    /// </summary>
    [PublicAPI]
    public static class StringUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly string DIR_SEPARATOR_STR = Path.DirectorySeparatorChar.ToString();

        private static Regex _regexSubDirectoryParameter;
        public const string PATTERN_QUESTION_IDENTIFIER = "[qQ][0-9]+([cC][0-9]+";
        private const string PATTERN_RESPONDENT_VALUES_TOKENIZER = @"{((RESPONDENT:[a-zA-Z0-9]+((:[a-zA-Z0-9]+)?)+)|(RANDOM)|(CURRENT_QUESTION)|(BASE_DIR)|(RESULT_FILE)|([qQ][0-9]+([cC][0-9]+)?(:(bool|rgb|float|time|size|filename|(date:[YyMmDdHhMmSs_-]+)))?)|(DATE:[YyMmDdHhMmSs_-]+)?)}";
        private const string PATTERN_PLAIN_VALUES_TOKENIZER = @"{((BASE_DIR)|(RESULT_FILE)|(RANDOM)|(DATE:[YyMmDdHhMmSs_-]+))}";
        private static Regex _regexPlainValues;
        private static Regex _regexDetectionTmpFiles;
        private const string PATTERN_DETECT_TEMP_FILE_TOKENS = @"{(TMP_IN|TMP_OUT|OUT|IN|IN_VAL):[a-zA-Z0-9\.\-_]+(:(JPG|PNG|MPC)(:[0-9]+)?)?}";

        /// <summary>
        /// Retourne le nom du fichier avec l'extension, en coupant la chaîne à partir du dernier séparateur de fichier.
        /// <br/>Fait également une vérification si le nom du fichier contient des paramètres HTTP GET, ce qui est possible
        /// quand on cherche un nom de fichier venant de l'internet (comme Instagram par exemple).
        /// </summary>
        /// <param name="file_src">Le chemin du fichier à analyser.</param>
        /// <param name="removeExtension">Indique qu'il faut enlever l'extension du nom de fichier.</param>
        /// <returns>Uniquement le nom du fichier (ou du dernier élément après le dernier séparateur). Sans paramètre HTTP GET s'ils existent.</returns>
        public static string GetFileName(string file_src, bool removeExtension = false)
        {
            if (file_src == null)
                return null;

            var i = file_src.LastIndexOf('/');

            if (i == -1)
                i = file_src.LastIndexOf('\\');

            if (i == -1)
                return file_src;

            var tentativeFileName = file_src.Substring(i + 1);
            // Peut contenir des arguments HTTP GET, comem par exemple "833111452_n.jpg?ig_cache_key=MTE4MDEwMzI1OTk5Mjk3NTYwNw%3D%3D.2"
            // Il faut donc enlever les possibles caractères.
            var index = tentativeFileName.IndexOf("?", StringComparison.InvariantCulture);
            if (index > 0)
            {
                tentativeFileName = tentativeFileName.Substring(0, index);
            }

            var tmp = GetFileName(tentativeFileName); // Tente d'enlever l'autre cas de figure en plus, dans la situation où on peut avoir les deux caractères.

            return !removeExtension ? tmp : tmp.Substring(0, tmp.LastIndexOf(".", StringComparison.InvariantCulture));
        }

        /// <summary>
        /// Retourne l'extension du fichier, en coupant la chaîne au dernier point lu.
        /// <br/>Si la chaîne est nulle, on retourne null. Si le fichier ne contient pas d'extension (aucun point), on retourne une chaîne vide.
        /// <br/>La case de l'extension est préservée.
        /// </summary>
        /// <param name="file_src">La chaîne à utiliser. Peut être un chemin complet.</param>
        /// <returns>Null si aucun paramère adéqueat, sinon l'extension (peut être une chaîne vide). Ne contient pas de point.</returns>
        public static string GetFileExtension(string file_src)
        {
            if (file_src == null)
                return null;

            var i = file_src.LastIndexOf('.');

            return i == -1 ? "" : file_src.Substring(i + 1);
        }

        /// <summary>
        /// Obtient le nom du dernier répertoire d'un chemin.
        /// Ce chemin ne doit pas terminer avec un caractère de séparation. S'il ne contient pas de cractère de séparation, on utilise
        /// l'argument comme retour.
        /// </summary>
        /// <param name="dirPath">Le chemin du répertoire.</param>
        /// <returns>Le nom du dernier répertoire dans le chemin. Sinon, le chemin complet.</returns>
        public static string GetLastDirName(string dirPath)
        {
            if (dirPath == null)
                return null;

            var i = dirPath.LastIndexOf('/');

            if (i == -1)
                i = dirPath.LastIndexOf('\\');

            if (i == -1)
                return dirPath;

            return dirPath.Substring(i + 1);
        }

        /// <summary>
        /// Retourne le répertoire complet du fichier, sans le nom de fichier lui-même.
        /// </summary>
        /// <param name="fullFilePath">Le chemin complet du fichier.</param>
        /// <returns>Le chemin répertoire du fichier.</returns>
        public static string GetDirName(string fullFilePath)
        {
            if (fullFilePath == null)
                return null;

            var i = fullFilePath.LastIndexOf('/');

            if (i == -1)
                i = fullFilePath.LastIndexOf('\\');

            if (i == -1)
                return fullFilePath;

            return fullFilePath.Substring(0, i);
        }

        /// <summary>
        /// Enlève l'extension d'un nom de fichier, si on en trouve un.
        /// N'enlève que la dernière extension. Si un fichier possède plusieures fois le caractère '.', cette
        /// méthode n'enlèvera que la dernière section.
        /// </summary>
        /// <param name="file_src">Le nom de fichier à analyser.</param>
        /// <returns>Le nom de fichier sans la dernière extension.</returns>
        public static string RemoveExtension(string file_src)
        {
            if (file_src == null)
                return null;

            var i = file_src.LastIndexOf('.');

            return i == -1 ? file_src : file_src.Substring(0, i);
        }

        public static string GetClassName(string full_class_name)
        {
            if (string.IsNullOrWhiteSpace(full_class_name)) return null;
            
            var i = full_class_name.LastIndexOf('.');

            return full_class_name.Substring(i + 1);
        }

        private static bool MatchString(string str, string regexstr)
        {
            if (str == null) return false;
            str = str.Trim();
            var pattern = new Regex(regexstr);

            return pattern.IsMatch(str);
        }

        public static bool IsValidIPAddress(string ipAddr)
        {
            // Allows four octets of numbers that contain values between 4 numbers in the IP address to 0-255 and are separated by periods
            const string regExPattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return MatchString(ipAddr, regExPattern);
        }

        /// <summary>
        /// Indique si une adresse IP est valide, et s'il obliger un numéro de port ou non.
        /// </summary>
        /// <param name="ipAddr">L'adresse IP</param>
        /// <param name="forcePort">S'il faut absolument une information de port.</param>
        /// <returns>TRUE si et seulement si l'adresse et le port (si existant) sont une adresse valide.</returns>
        public static bool IsValidIPAddressWithPort(string ipAddr, bool forcePort = false)
        {
            // Allows four octets of numbers that contain values between 4 numbers in the IP address to 0-255 and are separated by periods
            const string regExPattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(:[1-9][0-9]?[1-9]?[0-9]?[0-9]?)?$";
            if (!MatchString(ipAddr, regExPattern)) return false;

            // On vérifie maintenant la valeur maximale du port, s'il existe
            var split = ipAddr.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length <= 1 && !forcePort) return true;
            if (split.Length == 1) return false;
            if (!int.TryParse(split[1], out var res)) return false;

            // Voir ici pour les "magic numbers": https://en.wikipedia.org/wiki/Registered_port
            return res < 49151 && res > 1024;
        }

        public static bool IsValidServerPort(string port)
        {
            const string pattern = "^([1-9][0-9][1-9][0-9])$";
            var reg = new Regex(pattern, RegexOptions.Singleline | RegexOptions.ExplicitCapture);
            return reg.IsMatch(port);
        }

        public static bool IsValidStreamPort(string port)
        {
            const string pattern = "^([1-9][0-9][0-9][0-9])$";
            var reg = new Regex(pattern);
            return reg.IsMatch(port);
        }

        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            /*string pattern = @"^(\([2-9]\d{2}\)|[2-9]\d{2})[- ]?\d{3}[- ]?\d{4}$";*/ //819-456 4555 est valdie de meme...
            const string pattern = @"(^(\([2-9]\d{2}\)|[2-9]\d{2})[- ]?\d{3}[-]?\d{4}$)|(^(\([2-9]\d{2}\)|[2-9]\d{2})[ ]\d{3}[ ]\d{4}$)";
            //const string pattern = @"^\s*(?:\+?|(\+?(\d{1,3})))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*";
            var reg = new Regex(pattern);
            return reg.IsMatch(phoneNumber);
        }

        public static bool IsValidEmail(string emailAdr) {
            if (string.IsNullOrWhiteSpace(emailAdr)) return false;

            const string pattern = @"^(([^<>()[\]\\.,;:\s@\""]+"
                                   + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                                   + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                                   + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                                   + @"[a-zA-Z]{2,}))$";
            var reg = new Regex(pattern);
            return reg.IsMatch(emailAdr);
        }

        public static bool IsValidCanonPictureNumber(string port)
        {
            var pattern = "^(IMG_[0-9][0-9][0-9][0-9]((.jpg)|(.JPG)))$";
            var reg = new Regex(pattern);
            return reg.IsMatch(port);
        }

        public static bool IsValidNikonPictureNumber(string port)
        {
            var pattern = "^(DSC_[0-9][0-9][0-9][0-9]((.jpg)|(.JPG)))$";
            var reg = new Regex(pattern);
            return reg.IsMatch(port);
        }

        public static bool IsValidUSPostalCode(string port)
        {
            var pattern = "\\d{5}(-\\d{4})?";
            var reg = new Regex(pattern);
            return reg.IsMatch(port);
        }

        public static bool IsInteger(string integer)
        {
            return int.TryParse(integer, out _);
        }

        public static bool IsValidCanadaPostalCode(string postal_code)
        {
            const string pattern = "(^[ABCEGHJKLMNPRSTVXY]\\d[A-Z][ ]?\\d[A-Z]\\d$)|(^[ABCEGHJKLMNPRSTVXY]\\d[A-Z]-\\d[A-Z]\\d$)";
            var reg = new Regex(pattern);
            return reg.IsMatch(postal_code.ToUpper());
        }

        public static string EscapeXMLIlegalChar(string valueEscaped)
        {
            valueEscaped = valueEscaped.Replace("&", "&amp;");
            valueEscaped = valueEscaped.Replace("<", "&lt;");
            valueEscaped = valueEscaped.Replace(">", "&gt;");
            valueEscaped = valueEscaped.Replace("'", "&apos;");
            valueEscaped = valueEscaped.Replace("\"", "&quot;");

            return valueEscaped;
        }

        public static int GetStringWidth(Graphics g, Font f, string caracs)
        {
            return (int)g.MeasureString(caracs, f).Width;
        }

        public static int GetNumberOfLines(Graphics g, Font f, int maxWidth, string caracs)
        {
            var count = 1;
            try
            {
                //BUG FIX. ON ENLEVE 2 PIXELS POUR TENIR COMPTE DES BORDERS DES LABELS. 
                maxWidth = maxWidth - 2;

                var rect = g.MeasureString(caracs, f);
                if (rect.Width <= maxWidth)
                    return count;

                var index = caracs.LastIndexOf(' ');
                if (index == -1)
                    return count;

                var length = caracs.Length;
                var new_caracs = caracs.Substring(index + 1, length - (index + 1));
                var remainingCaracs = caracs.Substring(0, index);
                var newWordtoAdd = new_caracs;
                while (true)
                {
                    var width = (int)g.MeasureString(new_caracs, f).Width;
                    if (width <= maxWidth)
                    {
                        index = remainingCaracs.LastIndexOf(' ');
                        if (index == -1)
                        {
                            new_caracs = remainingCaracs + " " + new_caracs;
                            width = (int)g.MeasureString(new_caracs, f).Width;
                            if (width >= maxWidth)
                                count++;
                            break;
                        }
                        length = remainingCaracs.Length;
                        new_caracs = remainingCaracs.Substring(index + 1, length - (index + 1)) + " " + new_caracs;
                        newWordtoAdd = remainingCaracs.Substring(index, length - index);
                        remainingCaracs = remainingCaracs.Substring(0, index);
                    }
                    else
                    {
                        count++;
                        remainingCaracs += newWordtoAdd;
                        index = remainingCaracs.LastIndexOf(' ');
                        if (index == -1)
                            break;
                        length = remainingCaracs.Length;
                        new_caracs = remainingCaracs.Substring(index + 1, length - (index + 1));
                        remainingCaracs = remainingCaracs.Substring(0, index);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(@"error");
            }
            return count;
        }

        public static int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            var count = 0;
            var i = 0;
            while ((i = text.IndexOf(pattern, i, StringComparison.Ordinal)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        public static bool IsValidMinuteTime(string code)
        {
            var pattern = "^[0-9]+:[0-5][0-9]$";
            var reg = new Regex(pattern);
            return reg.IsMatch(code);
        }

        [PublicAPI]
        public static string[] SplitString(string toSplit, string delimiter)
        {
            var retour = toSplit?.Split(delimiter?.ToCharArray());

            return retour;
        }

        [Obsolete("Utiliser 'FormatEnumerable' à la place.")]
        public static string MergeString(object[] tab, string delimiter)
        {
            return MergeString(tab.AsEnumerable(), delimiter);
        }

        [Obsolete("Utiliser 'FormatEnumerable' à la place.")]
        [PublicAPI]
        public static string MergeString(IEnumerable<object> tab, string delimiter)
        {
            if (tab == null)
            {
                return null;
            }

            var sb = new StringBuilder();

            foreach (var s in tab)
            {
                if (delimiter != null && sb.Length > 0)
                {
                    sb.Append(delimiter);
                }

                sb.Append(s);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Génère un String de façon aléatoire en utilisant Base64 et le concept de GUID de C#
        /// </summary>
        /// <returns>Une chaîne de caractères aléatoire, mélangeant chiffres et caractères minuscules et majuscules.</returns>
        public static string generateRandomString()
        {
            var g = Guid.NewGuid();
            var GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            GuidString = GuidString.Replace("/", "");

            return GuidString;
        }

        /// <summary>
        /// Créer un point à partir d'un couple de valeur sous la forme "X, Y", à l'intérieur d'une chaîne de caractères.
        /// <br/>Les caractères délimiteurs sont ceux contenus dans StringUtil.STRING_DELIMITER_CHARS.
        /// </summary>
        /// <param name="readFrom">Une chaîne contenant un nombre entier, puis d'une séquence de caractère de séparation, puis d'un autre nombre entier.</param>
        /// <returns>Un point valide, sinon une exception.</returns>
        public static Point PointFromStringDefinition(string readFrom)
        {
            ReadIntegersFromString(readFrom, 2, STRING_DELIMITER_CHARS, out var result);

            return new Point(result[0], result[1]);
        }

        /// <summary>
        /// Créer une instance de "Size" à partir d'une chaîne de caractère sous la forme "X, Y", à l'intérieur d'une chaîne de caractères.
        /// <br/>Les caractères délimiteurs sont ceux contenus dans StringUtil.STRING_DELIMITER_CHARS.
        /// </summary>
        /// <param name="readFrom">Une chaîne contenant un nombre entier, puis d'une séquence de caractère de séparation, puis d'un autre nombre entier.</param>
        /// <returns>Un Size valide, sinon une exception.</returns>
        public static Size SizeFromStringDefinition(string readFrom)
        {
            ReadIntegersFromString(readFrom, 2, STRING_DELIMITER_CHARS, out var result);

            return new Size(result[0], result[1]);
        }
        
        /// <summary>
        /// Créer une instance de "Rectangle" à partir d'une chaîne de caractère sous la forme "X, Y", à l'intérieur d'une chaîne de caractères.
        /// <br/>Les caractères délimiteurs sont ceux contenus dans StringUtil.STRING_DELIMITER_CHARS.
        /// </summary>
        /// <param name="readFrom">Une chaîne contenant quatres nombres, séparés par des charactères de délimitation.</param>
        /// <returns>Un Size valide, sinon une exception.</returns>
        public static Rectangle RectangleFromStringDefinition(string readFrom)
        {
            ReadIntegersFromString(readFrom, 4, STRING_DELIMITER_CHARS, out var result);

            return new Rectangle(result[0], result[1], result[2], result[3]);
        }

        /// <summary>
        /// Créer une instance de "Color" à partir d'une chaîne de caractère sous la forme "A,R,G,B" ou "R,G,B", à l'intérieur d'une chaîne de caractères.
        /// <br/>Les caractères délimiteurs sont ceux contenus dans StringUtil.STRING_DELIMITER_CHARS.
        /// </summary>
        /// <param name="readFrom">Une chaîne contenant trois ou quatres nombres, séparés par des charactères de délimitation.</param>
        /// <returns>Une instance de Color valide, sinon une exception.</returns>
        public static Color ColorFromStringDefinition(string readFrom)
        {
            ReadIntegersFromString(readFrom, 3, STRING_DELIMITER_CHARS, out var result);
            return result.Length == 3
                       ? Color.FromArgb(result[0], result[1], result[2])
                       : Color.FromArgb(result[0], result[1], result[2], result[3]);
        }

        /// <summary>
        /// Lit une liste d'entiers à partir d'une chaine de caractères et une série de caractère de séparation.
        /// Lance une exception s'il y a des erreurs ou si la validation du nombre trouvé est échouée.
        /// </summary>
        /// <param name="readFrom">La chapine à lire.</param>
        /// <param name="minNumber">Le nombre minimal d'entiers à trouver.</param>
        /// <param name="delimiters">L'ensemble des caractères de délimitation possibles.</param>
        /// <param name="results">Le tableau résultant.</param>
        [PublicAPI]
        public static void ReadIntegersFromString(string readFrom, int minNumber, char[] delimiters, out int[] results)
        {
            if (string.IsNullOrWhiteSpace(readFrom))
            {
                throw new Exception("Cannot read integers out of a null or empty string.");
            }

            var data = readFrom.Split(delimiters, minNumber, StringSplitOptions.RemoveEmptyEntries);

            // Conversion et validation des points en même temps.
            var error = data.Length < minNumber;
            results = new int[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                error |= !int.TryParse(data[i], out results[i]);
            }

            if (error)
            {
                throw new Exception("Invalid string for making a list of integers: " + readFrom);
            }
        }

        /// <summary>
        /// Caractères utilisés pour délimiter les chaînes de caractères contenant une liste de valeurs.
        /// </summary>
        [PublicAPI]
        public static readonly char[] STRING_DELIMITER_CHARS = { ',', ';', ':', '|', 'x', 'X' };
        
        /// <summary>
        /// Retourne un tableau de valeurs entières à partir de valeurs lues dans une chaîne de caractère, délimitée par un caractère passé en paramètre.
        /// Si une erreur se produit, ou si on ne peut pas contruire un tableau avec le contenu, on journalise une erreur 
        /// puis on retourne null.
        /// </summary>
        /// <param name="dataString">La chaîne à analyser.</param>
        /// <param name="charSeparator">Le caractère de séparation</param>
        /// <returns>Un tableau si aucune erreur et valeurs compatibles, null sinon.</returns>
        public static int[] GetDataFromDelimitedString(string dataString, params char[] charSeparator)
        {
            if (string.IsNullOrWhiteSpace(dataString)) return null;

            var data = dataString.Split(charSeparator);
            try
            {
                var dataInt = new int[data.Length];
                for (var i = 0; i < data.Length; i++)
                {
                    dataInt[i] = int.Parse(data[i], CultureInfo.InvariantCulture);
                }

                return dataInt;
            }
            catch (Exception e)
            {
                log.Error(e, "Could not parse data as integers: data={0}", dataString);
                return null;
            }
        }

        /// <summary>
        /// Extrait les informations d'identifiant du fichier de la caméra.
        /// Ces identifiants sont les chaînes de caractère séparée par des '_' (underscore), hormis la dernière partie.
        /// En temps normal, le premier indice est l'auteur (sous forme d'entier, mais pas garantit), puis le numéro de série.
        /// Cela permet de faire une vérification si un groupe de caméra utilise un auteur similaire aux autres.
        /// </summary>
        /// <param name="fileName">Le nom de fichier à utiliser. On peut donner le chemin complet.</param>
        /// <returns>NULL si le paramètre est invalide, ou bien un tableau non-nul de tout les identifiants trouvés. Peut être vide.</returns>
        public static string[] ExtractUniqueIdsFromFileName(string fileName)
        {
            // Le format est "[Author]_[SN]_[UniqueId].jpg"
            if (fileName == null) return null;

            fileName = GetFileName(fileName); // La référence de l'appellant ne sera pas modifié

            var split = fileName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 2) return new string[0]; // Ne contient aucun identifiant outre son nom

            var result = new string[split.Length - 1];
            for (var i = 0; i < result.Length; i++)
                result[i] = split[i];

            return result;
        }

        /// <summary>
        /// Transforme un tableau de n'importe quoi en chaîne de caractère sur une ou plusieures lignes, avec un nombre arbitraire de tabulations pour chaque nouvelle ligne.
        /// </summary>
        /// <param name="input">Le tableau à formatter.</param>
        /// <param name="multipleLines">Indique s'il faut mettre ses informations sur de multiples lignes. Si faux, utilisera le séparateur donné en paramètre.</param>
        /// <param name="nbrSeparatorsInNewLine">Le nombre de séparateurs à mettre au début de chaque nouvelle ligne, si applicable.</param>
        /// <param name="separator">La chaîne de séparation entre chaque élément, ou au début de chaque ligne.</param>
        /// <param name="skipNullValue">Si les valeurs énumérée sont nulles, on saute par-dessus (aucun output). Si false, fait le output "NULL"</param>
        /// <param name="customToString">Si on veut une méthode de "ToString" personalisée (comme par exemple pour formatter un nombre), on peut passer un fonction en paramètre.</param>
        /// <returns>Le résultat du formatage.</returns>
        public static string FormatEnumerable(Array input, bool multipleLines = true, int nbrSeparatorsInNewLine = 0, 
            string separator = "\t", bool skipNullValue = true, Func<object, string> customToString = null)
        {
            if (input == null) return null;
            if (nbrSeparatorsInNewLine < 1) nbrSeparatorsInNewLine = 0;

            var sb = new StringBuilder();
            var first = true;
            var ite = input.GetEnumerator();
            while (ite.MoveNext())
            {
                var obj = ite.Current;
                if (obj == null && skipNullValue) continue;
                StringBuilderIteration(sb, obj, first, multipleLines, nbrSeparatorsInNewLine, separator, skipNullValue, customToString);
                first = false;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Transforme un tableau de Strings en chaîne de caractère sur une ou plusieures lignes, avec un nombre arbitraire de tabulations pour chaque nouvelle ligne.
        /// </summary>
        /// <param name="inputs">Le tableau de Strings.</param>
        /// <param name="multipleLines">Indique s'il faut mettre ses informations sur de multiples lignes. Si faux, utilisera le séparateur donné en paramètre.</param>
        /// <param name="nbrSeparatorsInNewLine">Le nombre de séparateurs à mettre au début de chaque nouvelle ligne, si applicable.</param>
        /// <param name="separator">La chaîne de séparation entre chaque élément, ou au début de chaque ligne.</param>
        /// <param name="skipNullValue">Si les valeurs énumérée sont nulles, on saute par-dessus (aucun output). Si false, fait le output "NULL"</param>
        /// <returns>Le résultat du formatage.</returns>
        public static string FormatEnumerable(string[] inputs, bool multipleLines = true, int nbrSeparatorsInNewLine = 0, string separator = "\t",
            bool skipNullValue = true)
        {
            return FormatEnumerable((Array) inputs, multipleLines, nbrSeparatorsInNewLine, separator, skipNullValue);
        }

        /// <summary>
        /// Transforme un énumérable d'objets en chaîne de caractère sur une ou plusieures lignes, avec un nombre arbitraire de tabulations pour chaque nouvelle ligne.
        /// </summary>
        /// <param name="inputs">La collection à formatter.</param>
        /// <param name="multipleLines">Indique s'il faut mettre ses informations sur de multiples lignes. Si faux, utilisera le séparateur donné en paramètre.</param>
        /// <param name="nbrSeparatorsInNewLine">Le nombre de séparateurs à mettre au début de chaque nouvelle ligne, si applicable.</param>
        /// <param name="separator">La chaîne de séparation entre chaque élément, ou au début de chaque ligne.</param>
        /// <param name="skipNullValue">Si les valeurs énumérée sont nulles, on saute par-dessus (aucun output). Si false, fait le output "NULL"</param>
        /// <param name="customToString">Si on veut une méthode de "ToString" personalisée (comme par exemple pour formatter un nombre), on peut passer un fonction en paramètre.</param>
        /// <returns>Le résultat du formatage.</returns>
        public static string FormatEnumerable(IEnumerable<object> inputs, bool multipleLines = true, int nbrSeparatorsInNewLine = 0, 
            string separator = "\t", bool skipNullValue = true, Func<object, string> customToString = null)
        {
            if (inputs == null) return null;
            if (nbrSeparatorsInNewLine < 1) nbrSeparatorsInNewLine = 0;

            var sb = new StringBuilder();
            var first = true;
            foreach (var obj in inputs)
            {
                if (obj == null && skipNullValue) continue;
                StringBuilderIteration(sb, obj, first, multipleLines, nbrSeparatorsInNewLine, separator, skipNullValue, customToString);
                first = false;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Transforme un énumérable d'objets en chaîne de caractère sur une ou plusieures lignes, avec un nombre arbitraire de tabulations pour chaque nouvelle ligne.
        /// </summary>
        /// <param name="inputs">La collection à formatter.</param>
        /// <param name="multipleLines">Indique s'il faut mettre ses informations sur de multiples lignes. Si faux, utilisera le séparateur donné en paramètre.</param>
        /// <param name="nbrSeparatorsInNewLine">Le nombre de séparateurs à mettre au début de chaque nouvelle ligne, si applicable.</param>
        /// <param name="separator">La chaîne de séparation entre chaque élément, ou au début de chaque ligne.</param>
        /// <param name="skipNullValue">Si les valeurs énumérée sont nulles, on saute par-dessus (aucun output). Si false, fait le output "NULL"</param>
        /// <param name="customToString">Si on veut une méthode de "ToString" personalisée (comme par exemple pour formatter un nombre), on peut passer un fonction en paramètre.</param>
        /// <returns>Le résultat du formatage.</returns>
        public static string FormatEnumerable<T>(IEnumerable<T> inputs, bool multipleLines = true, int nbrSeparatorsInNewLine = 0, 
            string separator = "\t", bool skipNullValue = true, Func<object, string> customToString = null)
        {
            if (inputs == null) return null;
            if (nbrSeparatorsInNewLine < 1) nbrSeparatorsInNewLine = 0;

            var sb = new StringBuilder();
            var first = true;
            foreach (var obj in inputs)
            {
                if (obj == null && skipNullValue) continue;
                StringBuilderIteration(sb, obj, first, multipleLines, nbrSeparatorsInNewLine, separator, skipNullValue, customToString);
                first = false;
            }

            return sb.ToString();
        }

        public static string FormatValueEnumerable<T>(IEnumerable<T> inputs, bool multipleLines = true, int nbrSeparatorsInNewLine = 0, 
            string separator = "\t", Func<T, string> customToString = null) where T : struct
        {
            if (inputs == null) return null;
            if (nbrSeparatorsInNewLine < 1) nbrSeparatorsInNewLine = 0;

            var sb = new StringBuilder();
            var first = true;
            foreach (var obj in inputs)
            {
                var str = customToString == null ? obj.ToString() : customToString(obj);
                StringBuilderEnumeration(multipleLines, nbrSeparatorsInNewLine, separator, first, sb, str);
                first = false;
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// Transforme le contenu d'un énumérateur d'objets en chaîne de caractère sur une ou plusieures lignes, avec un nombre arbitraire de tabulations pour chaque nouvelle ligne.
        /// </summary>
        /// <param name="inputs">La référence vers l'énumérateur</param>
        /// <param name="multipleLines">Indique s'il faut mettre ses informations sur de multiples lignes. Si faux, utilisera le séparateur donné en paramètre.</param>
        /// <param name="nbrSeparatorsInNewLine">Le nombre de séparateurs à mettre au début de chaque nouvelle ligne, si applicable.</param>
        /// <param name="separator">La chaîne de séparation entre chaque élément, ou au début de chaque ligne.</param>
        /// <param name="skipNullValue">Si les valeurs énumérée sont nulles, on saute par-dessus (aucun output). Si false, fait le output "NULL"</param>
        /// <param name="customToString">Si on veut une méthode de "ToString" personalisée (comme par exemple pour formatter un nombre), on peut passer un fonction en paramètre.</param>
        /// <returns>Le résultat du formatage.</returns>
        public static string FormatEnumerable(IEnumerator inputs, bool multipleLines = true, int nbrSeparatorsInNewLine = 0,
            string separator = "\t", bool skipNullValue = true, Func<object, string> customToString = null)
        {
            if (inputs == null) return null;
            if (nbrSeparatorsInNewLine < 1) nbrSeparatorsInNewLine = 0;

            var sb = new StringBuilder();
            var first = true;
            while (inputs.MoveNext())
            {
                var obj = inputs.Current;
                if (obj == null && skipNullValue) continue;

                StringBuilderIteration(sb, obj, first, multipleLines, nbrSeparatorsInNewLine, separator, skipNullValue, customToString);
                first = false;
            }

            return sb.ToString();
        }

        private static void StringBuilderIteration(StringBuilder sb, object obj, bool first, bool multipleLines = true, int nbrSeparatorsInNewLine = 0,
            string separator = "\t", bool skipNullValue = true, Func<object, string> customToString = null)
        {
            
            string str;
            switch (obj)
            {
                case null:
                    str = "NULL";
                    break;
                // Pseudo-Récursif ici
                case string[] strTab:
                    str = FormatEnumerable(strTab, multipleLines, nbrSeparatorsInNewLine + 1, separator, skipNullValue);
                    break;
                case Array strArr:
                    str = FormatEnumerable(strArr, multipleLines, nbrSeparatorsInNewLine + 1, separator, skipNullValue, customToString);
                    break;
                case IEnumerable<object> strEnume:
                    str = FormatEnumerable(strEnume, multipleLines, nbrSeparatorsInNewLine + 1, separator, skipNullValue, customToString);
                    break;
                default:
                    str = customToString != null ? customToString.Invoke(obj) : obj.ToString();
                    break;
            }

            StringBuilderEnumeration(multipleLines, nbrSeparatorsInNewLine, separator, first, sb, str);
        }

        private static void StringBuilderEnumeration(bool multipleLines, int nbrSeparatorsInNewLine, string separator, bool first, StringBuilder sb, string str)
        {
            if (!first)
            {
                if (multipleLines)
                {
                    for (var i = 0; i < nbrSeparatorsInNewLine; i++)
                    {
                        sb.Append(separator);
                    }
                }
                else
                {
                    sb.Append(separator);
                }
            }

            if (multipleLines)
            {
                sb.AppendLine(str);
            }
            else
            {
                sb.Append(str);
            }
        }

        /// <summary>
        /// Méthode générique pour retourner une valeur contenue dans un tableau (ou liste) d'un type particulier, et si l'index n'existe pas
        /// ou si le tableau est nul, retourne une valeur par défaut donnée par les paramètres.
        /// S'utilise de cette façons: 
        /// var tmp = GetArrayValueWithDefault(string[] tab, 1, "parDéfaut");
        /// </summary>
        /// <typeparam name="T">Le type. Est inféré par le compilateur</typeparam>
        /// <param name="list">La liste ou tableau de valeur indexées.</param>
        /// <param name="index">L'index à fouiller.</param>
        /// <param name="defaultValue">La valeur par défaut à utiliser.</param>
        /// <returns>La valeur trouvée, sinon, la valeur par défaut donnée en paramètre.</returns>
        public static T GetArrayValueWithDefault<T>(this IList<T> list, int index, T defaultValue)
        {
            if (list == null || !list.Any()) return defaultValue;
            if (list.Count <= index || index < 0) return defaultValue;
            var tmp = list[index];
            if (tmp == null || tmp.Equals(default(T))) return defaultValue;
            return list[index];
        }

        /// <summary>
        /// Méthode générique pour retourner une valeur contenue dans un tableau (ou liste) d'un type particulier, et si l'index n'existe pas
        /// ou si le tableau est nul, retourne une valeur par défaut, qui varie selon le type ('default(T)').
        /// S'utilise de cette façons: 
        /// var tmp = GetArrayValueWithDefault(string[] tab, 2);
        /// </summary>
        /// <typeparam name="T">Le type. Est inféré par le compilateur</typeparam>
        /// <param name="list">La liste ou tableau de valeur indexées.</param>
        /// <param name="index">L'index à fouiller.</param>
        /// <returns>La valeur trouvée, sinon, la valeur par défaut du type.</returns>
        [PublicAPI]
        public static T GetArrayValueWithDefault<T>(IList<T> list, int index)
        {
            if (list == null || !list.Any()) return default(T);
            if (list.Count > index && index >= 0) return list[index];
            return default(T);
        }

        /// <summary>
        /// Vérifie si un String contient des caractères qui ne sont pas ASCII, comme des accents.
        /// Peut être utile pour certains logiciels externes qui ont besoin d'un chemin disque sans accents.
        /// Voir ici: http://stackoverflow.com/questions/1522884/c-sharp-ensure-string-contains-only-ascii
        /// </summary>
        /// <param name="str">La chaîne à vérifier.</param>
        /// <returns>TRUE si des caractères illégaux sont trouvés.</returns>
        public static bool ContainsNonAscii(string str) {
            return Encoding.UTF8.GetByteCount(str) != str.Length;
        }
        
        /// <summary>
        /// Formate une valeur à virgule flottante pour un nombre maximal de décimales.
        /// </summary>
        /// <param name="nbrDecimal">Le nombre de décimales maximales.</param>
        /// <param name="value">La valeur à formatter.</param>
        /// <returns>Une chaîne de caractère de la valeur, formattée.</returns>
        public static string FormatFloat(int nbrDecimal, double value)
        {
            return string.Format(CultureInfo.InvariantCulture, '{' + "0:F" + MathUtil.Clamp(0, 10, nbrDecimal) + '}', value);
        }

        /// <summary>
        /// Formate une valeur à virgule flottante pour un nombre maximal de décimale.
        /// </summary>
        /// <param name="nbrDecimal">Le nombre de décimales maximales.</param>
        /// <param name="value">La valeur à formatter.</param>
        /// <returns>Une chaîne de caractère de la valeur, formattée.</returns>
        [PublicAPI]
        public static string FormatFloat(int nbrDecimal, decimal value)
        {
            return string.Format(CultureInfo.InvariantCulture, '{' + "0:F" + MathUtil.Clamp(0, 10, nbrDecimal) + '}', value);
        }

        /// <summary>
        /// Formatte un nombre de secondes (qui peut être fractionnaire) en un format que la ligne de commande de FFMPEG peut
        /// interpréter correctement. Le format est "0:0:0.00", qui représente "heures:minutes:secondes.fractionDeSecondes".
        /// </summary>
        /// <param name="timeInSeconds">La valeur temporelle, en secondes.</param>
        /// <returns>Une chaîne formattée de cette valeur temporelle, pour FFMPEG.</returns>
        public static string FormatTime(double timeInSeconds)
        {
            var tis = Convert.ToDecimal(timeInSeconds);

            var hoursDouble = tis - (tis % 3600);
            var modulo60 = tis % 60;
            var minutesDouble = tis - modulo60;
            var seconds = string.Format(CultureInfo.InvariantCulture, "{0:00.000}", modulo60);
            return FormatFloat(0, hoursDouble) + ':' + FormatFloat(0, minutesDouble) + ':' + seconds;
        }
        
        /// <summary>
        /// Formatte un objet Size dans un format lisible pour un filtre FFMPEG. Représentation de base d'une taille en format texte.
        /// Exemple: LxH
        /// Si l'objet est vide (0x0), retourne "480x480" par défaut.
        /// </summary>
        /// <param name="size">La taille à formatter.</param>
        /// <param name="defaultSizeStr">Valeur par défaut si la chaîne à analyser est vide.</param>
        /// <returns>Une valeur de taille 2D lisible pour un filter FFMPEG.</returns>
        public static string FormatSize(Size size, string defaultSizeStr = "480x480")
        {
            return !size.IsEmpty ? $"{size.Width}x{size.Height}" : defaultSizeStr;
        }

        /// <summary>
        /// Formatte une couleur à une forme "R,G,B[,A]", avec des valeur RGB sur un octet, soit [0..255]
        /// </summary>
        /// <param name="color">La couleur à formatter.</param>
        /// <param name="alpha">Indique s'il faut donner la valeur Alpha à la fin aussi., Faux par défaut</param>
        /// <returns>Les valeur RGB(A), séparées par des virgules.</returns>
        public static string FormatColor(Color color, bool alpha = false)
        {
            var str = $"{color.R},{color.G},{color.B}";
            if (alpha) str += $",{color.A}";
            return str;
        }

        /// <summary>
        /// Génère un string avec des jetons potentiels venant d'un répondant et/ou d'une liste de réponse.
        /// Si on veut utiliser un paramètre mais que ce dernier n'existe pas, ou est invalide, lance une exeption ou remplace la valeur par "NA", selon le cas. 
        /// La liste des Jetons possibles est:
        /// qXcY: représente un code ou X est le numéro de question, et Y le numéro de choix. Y est optionel si "c" est absent. Peut être suivi de ":float", ce qui indique que
        /// la valeur doit être convertit en float et sera convertie avec un format acceptable pour FFMPEG. Peut également être suivi de ":time", ce qui veut dire que la valeur
        /// est un double représentant des secondes, et sera transformé dans un format "hh:mm:ss.ff". En dernier, on a ":size", ou le string sera transformé en "LxH" s'il contient deux
        /// valeur entières séparée par un caractère de délimitation. Si suivi de ":date", on assume un autre caractère ':' suivi d'un code de formattage de date, comme pour l'option plus bas.
        ///  Autre option, ":filename", qui considère que la réponse est un chemin fichier absolu et que l'on veut uniquement le nom de fichier, similaire à "StringUtil.GetFileName(file)".
        /// Si la question ou sous-choix est inexistant, donne la valeur "NA".
        /// DATE: Sous la forme de "{DATE:FORMAT}" ou "FORMAT" est un code de format de date lisible par C#, par exemple "yyyy-MM-dd" pour "2017-09-08"
        /// Un format invalide lance une exception.
        /// CURRENT_QUESTION: Va chercher la réponse de la question présentement traitée (choix 1). Est utilisé dans le cas où on traite une liste de questions, ce qui implique
        /// une transformation pour plusieures questions différentes avec des fichiers de base différents.
        /// Lance une exception si question inexistante. TODO: CURRENT_QUESTION est à enlever le plus vite possible.
        /// RESPONDENT: Indique que l'on veut utiliser le répondant lui-même comme source d'information. Après le ':', on indique le nom (Exact, sensible à la case, car utilise la réflexion)
        /// de la propriété que l'on veut interroger. Si inexistant (soit le répondant, soit la propriété à interroger), lance une exception.
        /// RANDOM: Génère une chaîne de caractère haxadécimaux avec notre générateur interne, basé sur l'encryption BASE64.
        /// BASE_DIR: Représente un chemin de répertoire absolu. Utile dans un cas où il faut utiliser des fichiers de façon absolu, on peut créer une répertoire puis y référer dans un jeton,
        /// ce qui sauve à la fois de l'espace dans le jeton et est plus clair.
        /// RESULT_FILE: Un jeton spécial, qui est 
        /// </summary>
        /// <param name="resp">Le répondant à utiliser comme source de métadonnées.</param>
        /// <param name="tokenizedPattern">La chaîne, avec jetons possibles, à modifier. Si vide ou nulle, retourne une chaîne vide.</param>
        /// <param name="answerMap">Le dictionnaire de réponse.</param>
        /// <param name="currentQuestion">Le numéro de question actuel.</param>
        /// <param name="baseDir">Optionel. Un répertoire de base.</param>
        /// <param name="resultFile">Optionel. Un nom de fichier final.</param>
        /// <param name="errorTolerant">Indique qui si le questionnaire ne contient pas de référence vers la question, on ne soulève pas d'execption. Vrai par défaut.</param>
        /// <param name="nullStringValue">Chaîne à utiliser quand on détecte une valeur null, ou une réponse inexistante. Vide par défaut.</param>
        /// <returns>Une chaîne de caractères contenant la version modifiée de la chaîne selon les arguments fournis.</returns>
        public static string CreateStringFromTokenizedPatternRespondent([NotNull] string tokenizedPattern, [NotNull] Dictionary<QuestionIdentifier, Answer> answerMap,
            QuestionIdentifier currentQuestion = null, IRespondent resp = null, string baseDir = null, string resultFile = null, bool errorTolerant = true, string nullStringValue = "")
        {
            // TODO Retirer certains jetons uniques et générifier l'usage de ces jetons via une classe de configuration dynamique.
            // TODO Jetons à générifier: RESULT_FILE, BASE_DIR, 
            
            if (string.IsNullOrWhiteSpace(tokenizedPattern))
            {
                return "";
            }

            // Création de la commande sur une ligne. Enlève les sauts de ligne et les "whitespace" inutiles, met juste un espace entre chaque ligne.
            var sb = new StringBuilder();

            var preRegex = tokenizedPattern.Trim();

            // Exécution des REGEX. On les initialise de façon paresseuse (pour économiser de la mémoire si on n'utilise jamais cette méthode)
            if (_regexSubDirectoryParameter == null)
            {
                _regexSubDirectoryParameter = new Regex(PATTERN_RESPONDENT_VALUES_TOKENIZER, RegexOptions.Compiled);
            }

            var tokens = _regexSubDirectoryParameter.Matches(preRegex);
            sb.Clear();

            var lastIndex = 0;
            foreach (Match match in tokens)
            {
                // 1- Ajoute le substring d'avant le regex du string original, à partir du dernier indice de la chaîne originale
                var nextPart = preRegex.Substring(lastIndex, match.Index - lastIndex);
                sb.Append(nextPart);
                lastIndex = match.Index + match.Length; // Met à jour l'index pour le prochain match, en sautant le contenu du regex.

                // On transforme maintenant le regex en valeur utilisable.
                // 2- On analyse le match pour savoir de quel type de jeton il s'agit.
                var tokenValue = match.Value.Substring(1, match.Length - 2); // On enlève les paranthèses avant et après. "{VALUE}" => "VALUE"

                if (tokenValue.StartsWith("RANDOM", StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.Append(GenerateUniqueId.GetId());
                }
                else if (tokenValue.StartsWith("DATE", StringComparison.InvariantCultureIgnoreCase))
                {
                    var tmpValue = tokenValue.Split(':')[1];
                    sb.Append(DateTime.Now.ToString(tmpValue, CultureInfo.GetCultureInfo("fr-CA")));
                }
                else if (tokenValue.StartsWith("BASE_DIR", StringComparison.InvariantCultureIgnoreCase))
                {
                    var modDir = baseDir ?? DIR_SEPARATOR_STR;
                    if (!modDir.EndsWith(DIR_SEPARATOR_STR))
                    {
                        modDir += DIR_SEPARATOR_STR;
                    }
                    sb.Append(modDir);
                }
                else if (tokenValue.StartsWith("RESULT_FILE", StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.Append(resultFile ?? GenerateUniqueId.GetId() + ".tmp");
                }
                else if (tokenValue.StartsWith("RESPONDENT", StringComparison.InvariantCultureIgnoreCase))
                {
                    var split = tokenValue.Split(':');
                    // ReSharper disable once AccessToModifiedClosure
                    var prop = typeof(IRespondent).GetProperties().FirstOrDefault(p => p.Name.Equals(split[1]));
                    if (prop == null)
                    {
                        throw new Exception("Unknown property for type IRespondent: " + split[1]);
                    }
                    var propValue = prop.GetValue(resp).ToString();

                    var convArgs = split.Length <= 2 ? new ArraySegment<string>() : new ArraySegment<string>(split, 2, split.Length - 2);

                    sb.Append(ConvertTokenValue(convArgs, propValue));
                }
                else if (tokenValue.StartsWith("CURRENT_QUESTION", StringComparison.InvariantCultureIgnoreCase))
                {
                    var qi = currentQuestion;

                    if (!answerMap.TryGetValue(qi, out var a))
                    {
                        if (errorTolerant)
                        {
                            sb.Append("N/A");
                        } else throw new Exception($"Current question {qi} does not have answers.");
                    }
                    else
                    {
                        sb.Append(a.Response);
                    }
                }
                else
                {
                     sb.Append(ConvertQuestionTokenValue(answerMap, tokenValue, nullStringValue));
                }
            }
            var lastPart = preRegex.Substring(lastIndex);
            sb.Append(lastPart);
            
            return sb.ToString();
        }

        public static string CreateStringFromTokenizedPattern(string tokenizedPattern, Dictionary<string, string> inValues = null, string baseDir = null, string resultFile = null, bool errorTolerant = true, string nullStringValue = "")
        {
            // TODO Retirer certains jetons uniques et générifier l'usage de ces jetons via une classe de configuration dynamique.
            // TODO Jetons à générifier: RESULT_FILE, BASE_DIR, 
            
            if (string.IsNullOrWhiteSpace(tokenizedPattern))
            {
                return "";
            }

            // Création de la commande sur une ligne. Enlève les sauts de ligne et les "whitespace" inutiles, met juste un espace entre chaque ligne.
            var sb = new StringBuilder();

            var preRegex = tokenizedPattern.Trim();

            // Exécution des REGEX. On les initialise de façon paresseuse (pour économiser de la mémoire si on n'utilise jamais cette méthode)
            if (_regexPlainValues == null)
            {
                _regexPlainValues = new Regex(PATTERN_PLAIN_VALUES_TOKENIZER, RegexOptions.Compiled);
            }

            var tokens = _regexPlainValues.Matches(preRegex);
            sb.Clear();

            var lastIndex = 0;
            foreach (Match match in tokens)
            {
                // 1- Ajoute le substring d'avant le regex du string original, à partir du dernier indice de la chaîne originale
                var nextPart = preRegex.Substring(lastIndex, match.Index - lastIndex);
                sb.Append(nextPart);
                lastIndex = match.Index + match.Length; // Met à jour l'index pour le prochain match, en sautant le contenu du regex.

                // On transforme maintenant le regex en valeur utilisable.
                // 2- On analyse le match pour savoir de quel type de jeton il s'agit.
                var tokenValue = match.Value.Substring(1, match.Length - 2); // On enlève les paranthèses avant et après. "{VALUE}" => "VALUE"

                if (tokenValue.StartsWith("RANDOM", StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.Append(GenerateUniqueId.GetId());
                }
                else if (tokenValue.StartsWith("DATE", StringComparison.InvariantCultureIgnoreCase))
                {
                    var tmpValue = tokenValue.Split(':')[1];
                    sb.Append(DateTime.Now.ToString(tmpValue, CultureInfo.GetCultureInfo("fr-CA")));
                }
                else if (tokenValue.StartsWith("BASE_DIR", StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.Append(baseDir ?? "/");
                }
                else if (tokenValue.StartsWith("RESULT_FILE", StringComparison.InvariantCultureIgnoreCase))
                {
                    sb.Append(resultFile ?? GenerateUniqueId.GetId() + ".tmp");
                }
                else if (tokenValue.StartsWith("IN", StringComparison.InvariantCultureIgnoreCase))
                {
                     sb.Append(ConvertPlainTokenValue(inValues, tokenValue, nullStringValue));
                }
            }
            var lastPart = preRegex.Substring(lastIndex);
            sb.Append(lastPart);
            
            return sb.ToString();
        }

        private static string ConvertPlainTokenValue(Dictionary<string, string> inFiles, string tokenValue, string nullStringValue = "")
        {
            if (inFiles == null || inFiles.Count == 0) return nullStringValue;
            var splitToken = tokenValue.Split(':'); // Donne 1 ou deux éléments

            if (splitToken.Length < 2)
            {
                // Mauvais paramètre
                return nullStringValue;
            }

            inFiles.TryGetValue(splitToken[1], out var val);

            return val ?? nullStringValue;
        }

        /// <summary>
        /// Convertit un string en un QuestionIdentifier. Si le format est illisible, retourne null.
        /// </summary>
        /// <param name="arg">La chaîne de caractère à analyser.</param>
        /// <returns>Un identifiant non-null si tout s'est bien passé. Si une erreur s'est produite, retourne nul.</returns>
        public static QuestionIdentifier ConvertToQuestionIdentifier(string arg)
        {
            if (string.IsNullOrWhiteSpace(arg)) return null;

            try
            {
                var splitQuestion = arg.Split('c', 'C');

                var qNo = splitQuestion[0].Substring(1); // On enlève le "qQ" en tant que premier caractère.
                string cNo = null;
                if (splitQuestion.Length == 2)
                {
                    // On enlève le "cC" en tant que premier caractère.
                    cNo = splitQuestion[1];
                }

                var q = int.Parse(qNo);
                var c = 1;
                if (cNo != null) c = int.Parse(cNo);
                return new QuestionIdentifier(q,c);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ConvertQuestionTokenValue(Dictionary<QuestionIdentifier, Answer> answerMap, string tokenValue, string nullStringValue = "")
        {
            var splitToken = tokenValue.Split(':'); // Donne 1 ou deux éléments

            // Si le deuxième élément n'est pas explicitement non-nul et égal à "bool", ce n'est pas un bool check.
            var isBoolCheck = splitToken.Length > 1 && splitToken[1].Equals("bool");
            
            var qi = ConvertToQuestionIdentifier(splitToken[0]);
            
            // S'il n'existe pas de réponse dans le dictionnaire ...
            if (!answerMap.TryGetValue(qi, out var a))
            {
                // On retourne "false" si c'était une vérification booléene, sinon on retourne "nullStringValue" comme si la réponse était nulle.
                return isBoolCheck ? false.ToString().ToLower() : nullStringValue;
            }

            // Avec une réponse, quel que soit son contenu, un test booléen retourne "vrai".
            if (isBoolCheck)
            {
                return true.ToString().ToLower();
            }
            
            // Sinon, on convertit la valeur textuelle de la réponse.
            return ConvertTokenValue(new ArraySegment<string>(splitToken, 1, splitToken.Length - 1), a.Response, a, nullStringValue);
        }

        // ReSharper disable once UnusedParameter.Local
        private static string ConvertTokenValue(ArraySegment<string> conversionTokens, string textVal, Answer answer = null, string nullStringValue = "")
        {
            var val = textVal ?? nullStringValue;

            if (conversionTokens.Count <= 0 || string.IsNullOrWhiteSpace(conversionTokens.Array?[conversionTokens.Offset])) return val;

            // ReSharper disable once PossibleNullReferenceException
            var conv = conversionTokens.Array[conversionTokens.Offset];

            try
            {
                string convVal;
                if (conv.Equals("float", StringComparison.InvariantCultureIgnoreCase)) { convVal = FormatFloat(2, double.Parse(val, CultureInfo.InvariantCulture)); }
                else if (conv.Equals("rgb", StringComparison.InvariantCultureIgnoreCase)) { convVal = FormatColor(ColorFromStringDefinition(val)); }
                else if (conv.Equals("size", StringComparison.InvariantCultureIgnoreCase)) { convVal = FormatSize(SizeFromStringDefinition(val)); }
                else if (conv.Equals("time", StringComparison.InvariantCultureIgnoreCase)) { convVal = FormatTime(double.Parse(val, CultureInfo.InvariantCulture)); }
                else if (conv.Equals("filename", StringComparison.InvariantCultureIgnoreCase)) { convVal = GetFileName(val); }
                else if (conv.Equals("bool", StringComparison.InvariantCultureIgnoreCase)) { convVal = bool.Parse(val).ToString(); }
                else if (conv.Equals("date", StringComparison.InvariantCultureIgnoreCase))
                {
                    var date = DateTime.Parse(val, CultureInfo.GetCultureInfo("fr-CA"));
                    // Dans cette situation, splitToken contient la définition de la date ands son troisième indice.
                    convVal = date.ToString(conversionTokens.Array[conversionTokens.Offset + 1], CultureInfo.GetCultureInfo("fr-CA"));
                }
                else if (conv.Equals("timespan", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Conversion de type Timespan Java vers une DateTime valide; requiert un autre token pour le format de la date.
                    var date = DateUtil.FromJavaTimeToDateTime(val);
                    convVal = date.ToString(conversionTokens.Array[conversionTokens.Offset + 1], CultureInfo.GetCultureInfo("fr-CA"));
                }
                /* TODO On ne peut plus "deviner" le numéro de choix d'une question: il n'est désormais plus possible d'accéder à une question sans le choix.
                else if (conv.Equals("choiceno", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Utilise la valeur entière du numéro de choix. Si autre que 1 et plus, utilise la valeur "NA", pour "N/A".
                    // Requiert une instance valide de la réponse du répondant.
                    if (answer == null)
                    {
                        convVal = nullStringValue;
                    }
                    else
                    {
                        var num = answer.ChoiceNo;
                        convVal = num < 1 ? nullStringValue : answer.ChoiceNo.ToString();
                    }
                }
                */
                else throw new Exception($"Unknown {conv} conversion option.");
                val = convVal;
            }
            catch (Exception e)
            {
                throw new Exception($"Conversion into {conv} failed with value {val}", e);
            }

            return val;
        }

        /// <summary>
        /// Sépare les valeurs d'une chaîne de caractère sous l'assomption qu'il s'agit d'une propriété.
        /// En gros, à partir du premier caractère '=', ce qui est à gauche (exclusif) est la clé, ce qui est à droite (exclusif) est la valeur, même si cette dernière inclue
        /// elle-même un caractère '='.
        /// La chaîne clé peut être vide, dans le cas ou le cractère '=' est le premier.
        /// </summary>
        /// <param name="data">La chaîne à analyser.</param>
        /// <returns>NUll si le paramètre est nul ou n'est pas une propriété valide. Sinon, un tableau de deux valeurs: index 0 = clé, index 1 = valeur.</returns>
        public static string[] ExtractPropertyFromString(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return null;
            var splitIndex = data.IndexOf('=');
            if (splitIndex == -1) return null;

            var tab = new []
            {
                data.Substring(0, splitIndex),
                data.Substring(splitIndex + 1)
            };

            return tab;
        }

        /// <summary>
        /// Créer une fraction un utilisant une chaîne de caractère du format "num/dem". Tout autre format est rejeté et retourne nul.
        /// </summary>
        /// <param name="s">La chaîne à analyser.</param>
        /// <returns>La fraction créé. Nul si paramètre invalide.</returns>
        public static Fraction FractionFromStringDefinition(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var index = s.IndexOf('/');
            if (index == -1)
            {
                index = s.IndexOf(':');
            }
            if (index < 1) return null;
            if (!(int.TryParse(s.Substring(0, index), out var num) && int.TryParse(s.Substring(index + 1), out var dem))) return null;
            return new Fraction(num, dem);
        }

        /// <summary>
        /// Transforme un dictionnaire d'objets en chaîne de caractère sur une ou plusieures lignes, avec un nombre arbitraire de tabulations pour chaque nouvelle ligne.
        /// Chaque tuple de dictionnaire est formatté comme un tableau.
        /// </summary>
        /// <param name="dic">Le dictionnaire à formatter.</param>
        /// <param name="multipleLines">Indique s'il faut mettre ses informations sur de multiples lignes. Si faux, utilisera le séparateur donné en paramètre.</param>
        /// <param name="nbrSeparatorsInNewLine">Le nombre de séparateurs à mettre au début de chaque nouvelle ligne, si applicable.</param>
        /// <param name="separator">La chaîne de séparation entre chaque élément, ou au début de chaque ligne.</param>
        /// <returns>Le résultat du formatage.</returns>
        public static string FormatDictionary(IDictionary dic, bool multipleLines = true, int nbrSeparatorsInNewLine = 0, string separator = "\t")
        {
            if (dic == null) return null;
            if (nbrSeparatorsInNewLine < 1) nbrSeparatorsInNewLine = 0;

            var sb = new StringBuilder();
            var first = true;

            sb.Append('{');
            var iterator = dic.GetEnumerator();
            while (iterator.MoveNext())
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (multipleLines)
                    {
                        for (var i = 0; i < nbrSeparatorsInNewLine; i++)
                        {
                            sb.Append(separator);
                        }
                    }
                    else
                    {
                        sb.Append(separator);
                    }
                }

                sb.Append('{');
                sb.Append(iterator.Key);
                sb.Append(',');
                if (iterator.Value is IEnumerable<object> val)
                {
                    sb.Append(FormatEnumerable(val, multipleLines, nbrSeparatorsInNewLine > 0 ? nbrSeparatorsInNewLine + 1 : 0));
                }
                else if (iterator.Value is IDictionary dico)
                {
                    sb.Append(FormatDictionary(dico, multipleLines, nbrSeparatorsInNewLine > 0 ? nbrSeparatorsInNewLine + 1 : 0));
                }
                else if (iterator.Value is Array arr)
                {
                    sb.Append(FormatEnumerable(arr, multipleLines, nbrSeparatorsInNewLine > 0 ? nbrSeparatorsInNewLine + 1 : 0));
                }
                else
                {
                    sb.Append(iterator.Value);
                }
                sb.Append('}');
                
                if (multipleLines)
                {
                    sb.Append(Environment.NewLine);
                }
            }
            sb.Append('}');

            return sb.ToString();
        }

        /// <summary>
        /// Cette méthode identifie les marqueurs identifiants des fichiers temporaires. Elle ne fait pas de remplacement par jeton, elle ne fait
        /// que les identifier pour usage ultérieur. À cette fin, elle retourne un dictionnaire qui contient chaque type de jeton (par exemple, TMP ou OUT),
        /// puis, pour chaque catégorie, une liste des identifiants uniques. On fini avec un format optionnel.
        /// Par exemple, une ligne qui contient "{TMP_IN:1}, {TMP_IN:2}, {OUT:2:JPG}" va créer un
        /// dictionnaires contenant les infos suivantes (format JSON pour être compact): {{"TMP_IN", {"1","2"}}, {"OUT", {"2"}}}
        /// Un dictionnaire vide est parfaitement possible. Cela indique seulement que aucun jeton de fichier temporaire n'a été détecté.
        /// Cette méthode ignore les autres types de jetons.
        /// Cette méthode retourne un tableau vide si la chaîne est invalide (I.E. vide ou nulle).
        /// </summary>
        /// <param name="line">La ligne de commande à tester.</param>
        /// <returns>Un dictionnaire contenant les groupes de jetons et leurs identifiants détectés.</returns>
        public static IDictionary<FileTokenType, ISet<FileTokenInfo>> IdentifyTokenizedTemporaryFilesTag(string line)
        {
            var resultDico = new Dictionary<FileTokenType, ISet<FileTokenInfo>>();

            if (string.IsNullOrWhiteSpace(line))
            {
                return resultDico;
            }

            var preRegex = line.Trim();

            // Exécution des REGEX. On les initialise de façon paresseuse (pour économiser de la mémoire si on n'utilise jamais cette méthode)
            if (_regexDetectionTmpFiles == null)
            {
                _regexDetectionTmpFiles = new Regex(PATTERN_DETECT_TEMP_FILE_TOKENS, RegexOptions.Compiled);
            }

            var tokens = _regexDetectionTmpFiles.Matches(preRegex);

            foreach (Match match in tokens)
            {
                var token = new FileTokenInfo(match.Value);

                if (!resultDico.ContainsKey(token.TokenType))
                {
                    resultDico.Add(token.TokenType, new SortedSet<FileTokenInfo>());
                }
                resultDico[token.TokenType].Add(token);
            }

            // Terminé, on retourne la ligne.
            return resultDico;
        }

        /// <summary>
        /// Variation de la méthode "CreateStringFromTokenizedPattern" pour remplacer les jetons de fichiers temporaires par des chemins de fichiers complets.
        /// Ces chemin de fichiers, et leur identifiants, sont passés via deux dictionnaires: un pour des fichiers temporaire, l'autre pour des fichiers sortants.
        /// La raison de pourquoi deux dictionnaires est que les identifiants sont partagés entre tous les types de fichiers tempraires, mais pas avec les fichiers sortant.
        /// </summary>
        /// <param name="tokenCommand">La commande avec jetons</param>
        /// <param name="outFilesDic">Le dictionnaires des fichiers sortants.</param>
        /// <param name="tmpFilesDic">Le dictionnaire des fichiers temporaires.</param>
        /// <param name="InValues">Valeurs intrants.</param>
        /// <returns>La ligne de commande dont les jetons ont étés actualisés.</returns>
        /// TODO Générifier pour que l'on utilise des objets plus génériques, puis fusionner avec la méthode de from token plus haut.
        public static string CreateStringFromTokenizedPatternTemporaryFiles(string tokenCommand, Dictionary<string, FileInfo> outFilesDic, Dictionary<string, FileInfo> tmpFilesDic, Dictionary<string, string> InValues)
        {
            if (string.IsNullOrWhiteSpace(tokenCommand))
            {
                return "";
            }

            // Création de la commande sur une ligne. Enlève les sauts de ligne et les "whitespace" inutiles, met juste un espace entre chaque ligne.
            var sb = new StringBuilder();

            var preRegex = tokenCommand.Trim();

            // Exécution des REGEX. On les initialise de façon paresseuse (pour économiser de la mémoire si on n'utilise jamais cette méthode)
            if (_regexDetectionTmpFiles == null)
            {
                _regexDetectionTmpFiles = new Regex(PATTERN_DETECT_TEMP_FILE_TOKENS, RegexOptions.Compiled);
            }

            var tokens = _regexDetectionTmpFiles.Matches(preRegex);
            sb.Clear();

            var lastIndex = 0;
            foreach (Match match in tokens)
            {
                // 1- Ajoute le substring d'avant le regex du string original, à partir du dernier indice de la chaîne originale
                var nextPart = preRegex.Substring(lastIndex, match.Index - lastIndex);
                sb.Append(nextPart);
                lastIndex = match.Index + match.Length; // Met à jour l'index pour le prochain match, en sautant le contenu du regex.

                // On transforme maintenant le regex en valeur utilisable.
                var fileToken = new FileTokenInfo(match.Value);

                switch (fileToken.TokenType)
                {
                    case FileTokenType.TMP_OUT:
                    case FileTokenType.TMP_IN:
                        fileToken.ToCommandParameter(tmpFilesDic, sb);
                        break;
                    case FileTokenType.OUT:
                        fileToken.ToCommandParameter(outFilesDic, sb);
                        break;
                    case FileTokenType.IN:
                        fileToken.ToCommandParameter(InValues, sb);
                        break;
                    case FileTokenType.IN_VAL:
                        fileToken.ToCommandParameter(InValues, sb, true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            var lastPart = preRegex.Substring(lastIndex);
            sb.Append(lastPart);

            return sb.ToString();
        }

        /// <summary>
        /// Prend une chaîne de caractère, considère que c'est une répresentation d'un 'long' (Int64)
        /// et retourne le résultat en hexadécimal, dans une chaîne de caractère.
        /// Si la valeur de tampon gauche est invalide ou la chaîne est nulle ou invalide, on retourne null.
        /// </summary>
        /// <param name="str">La chaîne contenant un "long"</param>
        /// <param name="keepSameByteOrder">Indique qu'il ne faut pas faire une inversion des bytes (par groupe de deux bytes)</param>
        /// <param name="mustLeftPadTo">Le nombre minimal de caractères.
        /// Nécessaire dans les situation où il devrait y avouir des zéro non-significatifs à la gauche.
        /// Doit être un multiple de deux positif, zéro inclu.</param>
        /// <returns>La chaîne hexadécimale transformée.</returns>
        public static string ReverseOrderHexString([NotNull] string str, bool keepSameByteOrder = false, int mustLeftPadTo = 8)
        {
            if (mustLeftPadTo < 0 || mustLeftPadTo % 2 != 0) return null;
            if (string.IsNullOrWhiteSpace(str)) return null;
            var success = long.TryParse(str, out var tmp);
            if (!success) return null;
            var hex = $"{tmp:X}".PadLeft(mustLeftPadTo, '0');

            if (keepSameByteOrder) return hex;

            var list = new List<string>();
            for (var i = 0; i < hex.Length; i += 2)
            {
                var l = i >= hex.Length - 1 ? 1 : 2;
                list.Add(hex.Substring(i, l));
            }

            list.Reverse();

            var sb = new StringBuilder();
            foreach (var l in list)
            {
                sb.Append(l);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convertit un texte de numéro de téléphone de style européen vers notre style nord-américain.
        /// Notamment, enlève les caractères '+' et '-' aux extrémités, entres autres, et s'assure qu'il y a dix chiffres.
        /// S'assure ensuite de la validité en invoquant "IsValidPhoneNumber"
        /// </summary>
        /// <param name="phoneNumber">Le numéro à analyser.</param>
        /// <param name="validPhone">Si succès, retourne une chaîne non-vide.</param>
        /// <returns>Retourne TRUE si et seulement si, le résultat est un numéro de téléphone valide.</returns>
        public static bool ConvertEuropePhoneToNorthAmericanPhone(string phoneNumber, out string validPhone)
        {
            validPhone = null;
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return false;
            }

            var tmp = phoneNumber.Trim(' ', '-', '+');
            if (tmp.StartsWith("1")) tmp = tmp.Substring(1); // On enlève l'indicatif régional 1 si présent.
            if (!IsValidPhoneNumber(tmp)) return false;
            validPhone = tmp;
            return true;
        }

        /// <summary>
        /// Concatène une liste de chaîne de caractères dans une seule avec le minimum possible de fin de lignes.
        /// </summary>
        /// <param name="texts">La liste de texte a concaténer</param>
        /// <returns>Chaîne vide ou l'aggrégation avec sauts de lignes.</returns>
        public static string ConcatStringsMultiline([NotNull] IList<string> texts)
        {
            if (!texts.Any()) return "";

            var sb = new StringBuilder();

            // Cette logique de "append" ajoute les fins de ligne au besoin seulement, sans "End of Line" final.
            using (var e = texts.GetEnumerator())
            {
                e.MoveNext();
                sb.Append(e.Current);
                while (e.MoveNext())
                {
                    sb.AppendLine();
                    sb.Append(e.Current);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Cette méthode tente de détecter si le chemin de droite est d'origine *NIX, va assembler les deux ensemble correctement.
        /// Contrairement à ce qui est dit, la méthode de base Path.Combine() ne fonctionne pas correctement.
        /// Cette méthode va donc pallier à cette erreur.
        /// Si le string de droite est un chemin windows, la combinaison se fait correctement.
        /// </summary>
        /// <param name="baseDir">Le répertoire de base.</param>
        /// <param name="subPath">Le nom ou chemin de répertoire à ajouter.</param>
        /// <returns>Le chemin complet, correctement concaténé.</returns>
        public static string PathCombineFromUnix(string baseDir, string subPath)
        {
            if (subPath.Contains('/'))
            {
                subPath = subPath.Replace('/', '\\');
            }

            if (baseDir.EndsWith("\\") && subPath.StartsWith("\\"))
            {
                subPath = subPath.Remove(0, 1);
            }
            else if (!baseDir.EndsWith("\\") && !subPath.StartsWith("\\"))
            {
                subPath = "\\" + subPath;
            }

            return baseDir + subPath;
        }

        public static string GenerateGuidString()
        {
            var g = Guid.NewGuid();
            var GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=","");
            GuidString = GuidString.Replace("+","");
            GuidString = GuidString.Replace("/","");
            return GuidString;

        }
    }
}
