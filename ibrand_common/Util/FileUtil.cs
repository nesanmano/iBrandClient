using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util.Json;

namespace NomadLogic.Common.Util
{
    /// <summary>
    /// Classe utilitaire pour le traitement de fichiers.
    /// </summary>
    [PublicAPI]
    public static class FileUtil
    {
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        // ReSharper disable once InconsistentNaming
        public struct WIN32_FIND_DATA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll")]
        private static extern bool FindClose(IntPtr hFindFile);

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly string FILE_EXTENSION_JPG_FILTER = "*.jpg";
        public static readonly string FILE_EXTENSION_VIDEO_FILTER = "*.mp4";
        public static readonly string FILE_EXTENSION_GIF_FILTER = "*.gif";

        public static readonly string[] BASIC_IMAGE_FILE_EXTENSIONS =
        {
            ".jpg", ".jpeg", ".png"
        };

        /// <summary>
        /// Utilise InterOp pour rapidement vérifier si un répertoire est vide (sans tout itérer dedans).
        /// Si on retourne une valeur null, c'est que le répertoire est soit inexistant ou qu'une erreur a empéché la vérification.
        /// </summary>
        /// <param name="info">Le répertoire à vérifier.</param>
        /// <returns>NULL si l'opération est impossible. TRUE si le répertoire est complétement vide. FALSE s'il existe mais contient quelque chose.</returns>
        public static bool? CheckDirectoryEmpty_Fast(this DirectoryInfo info)
        {
            return info == null ? null : CheckDirectoryEmpty_Fast(info.FullName);
        }

        /// <summary>
        /// Utilise InterOp pour rapidement vérifier si un répertoire est vide (sans tout itérer dedans).
        /// Si on retourne une valeur null, c'est que le répertoire est soit inexistant ou qu'une erreur a empéché la vérification.
        /// </summary>
        /// <param name="path">Le chemin à vérifier.</param>
        /// <returns>NULL si l'opération est impossible. TRUE si le répertoire est complétement vide. FALSE s'il existe mais contient quelque chose.</returns>
        public static bool? CheckDirectoryEmpty_Fast(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!Directory.Exists(path)) return null;
            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path += "*";
            else
                path += Path.DirectorySeparatorChar + "*";

            var findHandle = FindFirstFile(path, out var findData);

            if (findHandle == INVALID_HANDLE_VALUE) return null;
            try
            {
                var empty = true;
                do
                {
                    if (findData.cFileName != "." && findData.cFileName != "..")
                        empty = false;
                } while (empty && FindNextFile(findHandle, out findData));

                return empty;
            }
            finally
            {
                FindClose(findHandle);
            }
        }

        [PublicAPI]
        public static Image LoadImage(string path)
        {
            return LoadBitmap(path);
        }
        
        public static IEnumerable<string> FilterFiles(string path, SearchOption so, params string[] exts) {
            return
                Directory
                    .EnumerateFiles(path, "*.*", so)
                    .Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
        }

        public static IEnumerable<string> FilterFiles(string path, params string[] exts) {
            return
                Directory
                    .EnumerateFiles(path, "*.*")
                    .Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
        }

        public static IEnumerable<FileInfo> FilterFiles(this DirectoryInfo dirInfo, SearchOption so, Func<FileInfo, bool> filterFunc, params string[] exts)
        {
            var firstPass = dirInfo.EnumerateFiles("*.*", so)
                .Where(file => exts.Any(x => file.Name.EndsWith(x, StringComparison.OrdinalIgnoreCase)));

            if (log.IsTraceEnabled)
            {
                var tmp = firstPass.ToList();
                firstPass = tmp;
                log.Trace("FilterFile: Type Pass\n\t" + StringUtil.FormatEnumerable(tmp.Select(s => $"{s.Name,-30} : {s.LastWriteTime:yyyy-MM-dd HH:mm:ss.fff}"), true, 1));

                if (filterFunc != null)
                {
                    var whereTmp = tmp.Where(filterFunc);
                    log.Trace("FilterFile: Custom Filter Pass\n\t" + StringUtil.FormatEnumerable(whereTmp.Select(s => $"{s.Name,-30} : {s.LastWriteTime:yyyy-MM-dd HH:mm:ss.fff}"), true, 1));
                }
            }

            return filterFunc == null ? firstPass : firstPass.Where(filterFunc);
        }

        public static IEnumerable<FileInfo> FilterFiles(this DirectoryInfo dirInfo, params string[] exts) {

            return FilterFiles(dirInfo, SearchOption.TopDirectoryOnly, null, exts);
        }

        public static IEnumerable<FileInfo> FilterFiles(this DirectoryInfo dirInfo) {

            return FilterFiles(dirInfo, BASIC_IMAGE_FILE_EXTENSIONS);
        }

        public static IEnumerable<FileInfo> FilterFiles(this DirectoryInfo dirInfo, SearchOption so, Func<FileInfo, bool> filterFunc = null) {

            return FilterFiles(dirInfo, so, filterFunc, BASIC_IMAGE_FILE_EXTENSIONS);
        }

        /// <summary>
        /// Créé un filtre qui indentifie les fichier vieux de tout au plus l'argument en paramètre.
        /// </summary>
        /// <param name="maxTime">Temp maximal de vieillesse.</param>
        /// <returns>Une fonction filtre.</returns>
        public static Func<FileInfo, bool> FileDateFilterFunction(TimeSpan maxTime)
        {
            return info =>
            {
                var now = DateTime.Now;

                if (log.IsTraceEnabled)
                {
                    log.Trace("Max age check: {0} >= ({1} - {2})", info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), now.ToString("yyyy-MM-dd HH:mm:ss.fff"), maxTime.ToString("G"));
                }
                
                return info.LastWriteTime >= DateTime.Now - maxTime;
            };
        }

        /// <summary>
        /// Créer une fonction filtre pour les fichiers créés entre une date précise et une date maximale.
        /// </summary>
        /// <param name="minDate">La date minimale, exclusive.</param>
        /// <param name="maxDate">La date maximale, inclusive. Si null, on utilise NOW.</param>
        /// <returns>Une fonction filtre.</returns>
        public static Func<FileInfo, bool> FileDateFilterFunction(DateTime minDate, DateTime? maxDate = null)
        {
            var max = maxDate ?? DateTime.Now;
            if (minDate > max) return info => false;

            return info =>
            {
                if (log.IsTraceEnabled)
                {
                    log.Trace("Date check: {0} < {1} <= {2}", minDate.ToLongDateString(), info.LastWriteTime.ToLongDateString(), max.ToLongDateString());
                }

                return info.LastWriteTime > minDate && info.LastWriteTime <= max;
            };
        }

        /// <summary>
        /// Charge un fichier d'image dans un Bitmap GDI+.
        /// </summary>
        /// <param name="filePath">Le chemin du fichier.</param>
        /// <param name="resolution">La résolution d'écran voulu. 300 DPI par défaut, écrase les informations du fichier.</param>
        /// <param name="newSize">Indique si on veut une taille précise pour l'image via la méthode de copie de la classe Bitmap.</param>
        /// <returns>Peut retourner null si la fichier est invalide (fichier vide, invalide, pas un bitmap, etc..)</returns>
        [PublicAPI]
        public static Bitmap LoadBitmap(string filePath, float resolution = 300.0f, Size? newSize = null)
        {
            if (!File.Exists(filePath)) { return null; }

            Bitmap result;
            Bitmap tmpRef;

            try
            {
                result = new Bitmap(filePath);
                tmpRef = result;
                result.SetResolution(resolution, resolution);
                
                // teste si on doit redimensionner l'image immédiatement.
                if (newSize != null && !newSize.Value.IsEmpty && !newSize.Value.Equals(result.Size))
                {
                    var tmp = new Bitmap(result, newSize.Value);
                    tmp.SetResolution(resolution, resolution);
                    result.Dispose();
                    result = tmp;
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Could not load Bitmap from file: {0}", filePath);
                return null;
            }

            // ce processus enleve le lock sur le fichier src du bitmap si elle est deja charge une premiere fois.
            if (result != tmpRef) return result;
            
            var tmp2 = new Bitmap(result);
            tmp2.SetResolution(resolution, resolution);
            result.Dispose();
            
            return tmp2;
        }

        /// <summary>
        /// Charge de multiples fichiers d'un seul coup. Il n'y a pas d'option de résolution ou de nouvelle taille.
        /// </summary>
        /// <param name="filePaths">Les fichiers d'image à charger.</param>
        /// <param name="ct">Le jeton de cancellation.</param>
        /// <returns>Une liste non nulle des résultats. Peut contenir des instances nulles, selon les règles de LoadBitmap()</returns>
        public static async Task<List<Bitmap>> LoadMultipleBitmaps(IList<string> filePaths, CancellationToken ct)
        {
            if (!filePaths?.Any() ?? true)
            {
                return new List<Bitmap>();
            }

            var tasks = filePaths.Select(path => Task.Run(() => LoadBitmap(path), ct)).ToList();

            var results = await Task.WhenAll(tasks);
            return results?.ToList() ?? new List<Bitmap>();
        }

        
        /// <summary>
        /// Détruit un répertoire et tous les fichiers et sous-répertoires.
        /// <br/>Ne demande aucune confirmation! À utiliser avec soin.
        /// </summary>
        /// <param name="folderPath">Le chemin du répertoire.</param>
        public static void DeleteFolderContent(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) return;
            var dir = new DirectoryInfo(folderPath);

            if (!dir.Exists) return;

            foreach (var fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (var di in dir.GetDirectories())
            {
                DeleteFolderContent(di.FullName);
            }

            dir.Delete();
        }

        /// <summary>
        /// Trouve tous les fichiers d'un répertoire et de ses sous-répertoires qui sont des images JPEG ou PNG. Met ces fichiers dans une liste.
        /// <br/>Utilise la récursion.
        /// <br/>Sort immédiatement si les arguments sont nuls, ou que le répertoire est invalide ou n'existe pas.
        /// </summary>
        /// <param name="fileList">La liste à remplir.</param>
        /// <param name="currentDir">Le répertoire courant.</param>
        [Obsolete]
        public static void RecursiveImageFileFinder(List<FileInfo> fileList, string currentDir)
        {
            if (fileList == null || string.IsNullOrWhiteSpace(currentDir) || !Directory.Exists(currentDir))
            {
                return;
            }

            var dir = new DirectoryInfo(currentDir);
            var files = dir.EnumerateFiles();
            fileList.AddRange(files.Where(f => BASIC_IMAGE_FILE_EXTENSIONS.Contains(f.Extension, StringComparer.OrdinalIgnoreCase)));
            foreach (var direc in Directory.EnumerateDirectories(currentDir))
            {
                RecursiveImageFileFinder(fileList, direc);
            }
        }

        /// <summary>
        /// Obtient le répertoire complet d'un fichier, si existant.
        /// </summary>
        /// <param name="fileSrc"></param>
        /// <returns></returns>
        public static string GetFolderName(string fileSrc)
        {
            try
            {
                return new FileInfo(fileSrc).DirectoryName;
            }
            catch (Exception e)
            {
                log.Warn(e);
                return null;
            }
        }

        /// <summary>
        /// Tente de trier des noms de fichier selon la situation où le nom sans extension de ces fichiers est un nombre entier.
        /// <br/>Si c'est le cas, on trie selon l'ordre naturel de ces nombres. Sinon, on fait une comparaison de chaîne ordinale
        /// des noms entiers.
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns></returns>
        public static int SortFileFunctionAsNumbers(FileInfo file1, FileInfo file2)
        {
            var fileFullName1 = file1.FullName;

            var fileName1 = StringUtil.GetFileName(fileFullName1);
            var fileNameWoExt1 = StringUtil.RemoveExtension(fileName1);

            var no1 = -1;

            if (StringUtil.IsInteger(fileNameWoExt1))
                no1 = int.Parse(fileNameWoExt1);

            var fileFullName2 = file2.FullName;

            var fileName2 = StringUtil.GetFileName(fileFullName2);
            fileNameWoExt1 = StringUtil.RemoveExtension(fileName2);

            var no2 = -1;

            if (StringUtil.IsInteger(fileNameWoExt1))
                no2 = int.Parse(fileNameWoExt1);

            if (no1 != -1 && no2 != -1)
                return no1.CompareTo(no2);

            return string.Compare(fileName1, fileName2, StringComparison.Ordinal);
        }

        public static IEnumerable<FileInfo> GetFilesByExtensionsTodayOrderByDate(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));
            var files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension) && DateTime.Now.DayOfYear == f.CreationTime.DayOfYear).OrderBy(f => f.CreationTime);
        }

        public static IEnumerable<FileInfo> GetFilesByExtensionsTodayOrderByName(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));
            var files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension) && DateTime.Now.DayOfYear == f.CreationTime.DayOfYear).OrderBy(f => f.Name);
        }

        /// <summary>
        /// Cette méthode est une alternative pour la méthode Directory.GetFiles, car cette dernière ne supporte pas de multiples filtres séparés
        /// par le caractère "|".
        /// <br/>C'est un "wrapper" autour de la méthode pour correctement utiliser ce comportement.
        /// <br/>voir: <a href="https://social.msdn.microsoft.com/Forums/vstudio/en-US/b0c31115-f6f0-4de5-a62d-d766a855d4d1/directorygetfiles-with-searchpattern-to-get-all-dll-and-exe-files-in-one-call?forum=netfxbcl">ici</a>
        /// pour plus d'information.
        /// </summary>
        /// <param name="path">Le chemin</param>
        /// <param name="searchPattern">Le pattern. Example: "*.jpg|*.png"</param>
        /// <param name="searchOption">Les options de recherche. Si null, on utilise "SearchOption.TopDirectoryOnly"</param>
        /// <param name="sortFiles">Indique si on veut trier les fichier une fois obtenus, via la comparaison naturelle.</param>
        /// <returns>Un tableau non-null des fichiers du répertoire. Vide </returns>
        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption = SearchOption.TopDirectoryOnly, bool sortFiles = true)
        {
            var searchPatterns = searchPattern.Split('|');
            var files = new List<string>();
            foreach (var sp in searchPatterns)
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            if (sortFiles) files.Sort();
            return files.ToArray();
        }

        /// <summary>
        ///
        ///Cette methode permet de lire un fichier .properties et d'en extraire les cles/valeurs
        /// </summary>
        /// <param name="path"></param> le path pointant sur le fichier de properties a lire
        /// <returns></returns>
        public static Dictionary<string,string> ReadPropertiesFile(string path)
        {
            // FileInfo propertiesFile = new FileInfo(path);
            var val = new Dictionary<string, string>();
            var readHead = new StreamReader(path);
            string line;
            while ((line = readHead.ReadLine())!="" && line!=null)
            {
                var tmp = line.Split('=');

                val.Add(tmp[0],tmp[1]);
                
            }

            return val;

        }
        
        /// <summary>
        /// Cherche une liste de FileInfo venant d'un répertoire précis, ayant été créés au moins X milisecondes avant la demande,
        /// avec une liste d'extensions pour les fichiers visés.
        /// La liste est ordonnée selon la date de création.
        /// </summary>
        /// <param name="dir">Le répertoire d'où lancer la recherche.</param>
        /// <param name="delay">Le nombre minimal de milisecondes que le fichier doit exister avant que la recherche ne soit lancée.</param>
        /// <param name="extensions">La liste des extensions. DOIT contenir le point!</param>
        /// <returns>La liste ordonnée selon la date des fichiers respectant les conditions données.</returns>
        public static IEnumerable<FileInfo> GetFilesByExtensionsOrderByDate(DirectoryInfo dir, int delay, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException(nameof(extensions));
            var files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension) && DateTime.Now.Subtract(f.CreationTime).TotalMilliseconds > delay).OrderBy(f => f.CreationTime);
        }

        /// <summary>
        /// Cherche une liste de FileInfo venant d'un répertoire précis, ayant été créés au moins X milisecondes avant la demande,
        /// avec une liste d'extensions pour les fichiers visés.
        /// La liste est ordonnée selon le nom du fichier.
        /// </summary>
        /// <param name="dir">Le répertoire d'où lancer la recherche.</param>
        /// <param name="extensions">La liste des extensions. DOIT contenir le point!</param>
        /// <returns>La liste ordonnée selon le nom des fichiers respectant les conditions données.</returns>
        public static IEnumerable<FileInfo> GetFilesByExtensionsOrderByName(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
            if (dir == null) throw new ArgumentNullException(nameof(dir));

            dir.Refresh();
            if (!dir.Exists) return new FileInfo[0];

            var files = dir.EnumerateFiles();

            var orderedList = files.Where(f => extensions.Contains(f.Extension)).ToList();
            orderedList.Sort(SortFileFunctionAsNumbers);

            return orderedList;
        }
        
        /// <summary>
        /// Méthode de comparaison pour noms de fichiers, qui sont généralement des fichiers générés pour des groupement des caméras.
        /// Cela implique soit qu'ils ont des identifiants d'auteur dans leur nom, soit leur nom est un nombre quand on enlève l'extension.
        /// On détecte ces cas de figure et on effectue la bonne comparaison dans cette méthode.
        /// </summary>
        /// <param name="x">Premier fichier.</param>
        /// <param name="y">Deuxième fichier.</param>
        /// <returns>Un résultat analogue à "Compare"</returns>
        public static int CompareFileNames(string x, string y)
        {
            var id1 = StringUtil.ExtractUniqueIdsFromFileName(x);
            var id2 = StringUtil.ExtractUniqueIdsFromFileName(y);

            string idX;
            string idY;

            // Soit le premier identifiant (auteur) ou le nom du fichier sans l'extension.
            if (id1.Length >= 1 && id2.Length >= 1)
            {
                idX = id1[0];
                idY = id2[0];
            }
            else
            {
                idX = StringUtil.RemoveExtension(StringUtil.GetFileName(x));
                idY = StringUtil.RemoveExtension(StringUtil.GetFileName(y));
            }

            // Si les deux identifiants peuvent être convertits en entier, on compare ces entiers.
            if (int.TryParse(idX, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intX) &&
                int.TryParse(idY, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intY))
            {
                return intX - intY;
            }

            // Sinon on fait une comparaison des deux identifiants en ignorant la case.
            return string.Compare(idX, idY, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Trie les éléments d'une liste en considérant que les noms de ficheirs sont issus d'une série pour Camera Ring.
        /// </summary>
        /// <param name="files">La collection de fichier à trier.</param>
        public static void SortFileCollection(List<string> files)
        {
            files.Sort(CompareFileNames);
        }

        /// <summary>
        /// Trie les éléments d'une liste en considérant que les noms de ficheirs sont issus d'une série pour Camera Ring.
        /// </summary>
        /// <param name="files">La collection de fichier à trier.</param>
        public static void SortFileCollection(List<FileInfo> files)
        {
            files.Sort((arg1, arg2) => CompareFileNames(arg1.FullName, arg2.FullName));
        }

        /// <summary>
        /// Trie les éléments d'une liste en considérant que les noms de fichiers sont issus d'une série pour Camera Ring.
        /// </summary>
        /// <param name="files">La collection de fichiers à trier.</param>
        public static void SortFileCollection(string[] files)
        {
            Array.Sort(files, CompareFileNames);
        }

        /// <summary>
        /// Créer (ou écrase) un fichier zip qui contiendra le contenu d'un répertoire donné. On met dans le zip uniquement les
        /// fichiers racine, et on ignore les sous-répertoires.
        /// </summary>
        /// <param name="directory">Le répertoire à "zipper".</param>
        /// <param name="zipFileName">Le chemin du fichier ZIP à créer.</param>
        public static void CreateZipFileFromDirectory(DirectoryInfo directory, string zipFileName)
        {
            if (File.Exists(zipFileName))
            {
                File.Delete(zipFileName);
            }
            var picturesToUse = directory.GetFiles();
            using (var zip = new ZipFile())
            {
                zip.AddFiles(picturesToUse.Select(file => file.FullName), "");
                zip.Save(zipFileName);
            }
        }

        /// <summary>
        /// Copie un flux d'octets dans un fichier. Le flux est fermé à la fin du traitement, quel que soit son résultat.
        /// Il est recommandé d'augmenter le temps maximal de copie, particulièrement dans les cas où on fait un transfert de fichier venant d'un
        /// flux provenant d'une requête web.
        /// Si le temps maximal est mis à nul, on ne fait pas de vérification temporelle pour stopper le transfert.
        /// </summary>
        /// <param name="fileStream">Le flux utilisé pour créer le fichier.</param>
        /// <param name="fileName">Le nom de fichier qui sera créé avec le flux.</param>
        /// <param name="maxTimeMilliseconds">Le temps maximal impartit pour la copie, en milisecondes. 2000 par défaut.</param>
        /// <param name="ct">Jeton de cancellation optionel, dans les situation où on a une cancellation possible.</param>
        /// <returns>Si la copie de flux s'est déroulée sans encombre et sans aucune cancellation ni dépassement de temps.</returns>
        public static bool ReadStreamToFile(Stream fileStream, string fileName, uint? maxTimeMilliseconds = 2000, CancellationToken? ct = null)
        {
            if (fileStream == null || string.IsNullOrWhiteSpace(fileName)) return false;

            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using (var f = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var buffer = new byte[4096];
                    var t = DateTime.Now;
                    var tf = DateTime.Now.AddMilliseconds(maxTimeMilliseconds ?? 1000); // Si nul, on met une valeur juste pour dire que la comparaison sera toujours vrai.
                    var readCount = 0;
                    while (t < tf && (!ct?.IsCancellationRequested ?? true))
                    {
                        readCount = fileStream.Read(buffer, 0, buffer.Length);
                        f.Write(buffer, 0, readCount);
                        if (maxTimeMilliseconds.HasValue) t = DateTime.Now;
                        if (readCount == 0) break;
                    }

                    if (maxTimeMilliseconds.HasValue && t > tf && readCount > 0) return false; // Si il y avait encore une lecture faite lors de l'arrêt temporel, dans le cas d'un arrêt temporel.
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Could not copy stream to file: " + fileName);
                return false;
            }
            finally
            {
                fileStream.Dispose();
            }

            return !ct?.IsCancellationRequested ?? true; // Si on a cancellé, on retourne faux.
        }

        /// <summary>
        /// Lit le contenu d'un fichier texte, et retourne son contenu en un string qui ne contient pas de retour à la ligne.
        /// En somme, enlève les 'crlf' d'un fichier texte, ainsi que les espaces avant et après les derniers/premiers caractères
        /// qui ne sont pas du "whitespace".
        /// </summary>
        [PublicAPI]
        public static string ReadFileToStringSingleLine(string path)
        {
            var rawFileData = File.ReadAllLines(path);

            // Création de la commande sur une ligne. Enlève les sauts de ligne et les "whitespace" inutiles, met juste un espace entre chaque ligne.
            var sb = new StringBuilder();
            var firstLine = true;
            foreach (var line in rawFileData) {
                if (firstLine) { firstLine = false; }
                else { sb.Append(' '); }
                sb.Append(line.Trim());
            }

            return sb.ToString();
        }
        
        /// <summary>
        /// Détermine si le fichier est une image valide ou on en tentant d'en faire un Bitmap à l'interne.
        /// </summary>
        /// <param name="file">Le fichier à analyser.</param>
        /// <returns>TRUE si et seulement si on est capable de construire un Bitmap avec le fichier à analyser.</returns>
        public static bool IsImageFile(string file)
        {
            var isImage = false;

            if (!File.Exists(file)) { return false; }

            try
            {
                using (LoadBitmap(file)) { } // Ferme automatiquement le fichier
                isImage = true;
            }
            catch (Exception)
            {
                /* Die Nice */
            }

            return isImage;
        }

        /// <summary>
        /// Cette méthode traite un fichier pour en faire une version encodée en "Base64".
        /// La transformation d'un fichier en ce format est généralement utilisé pour le transfert HTTP, car un
        /// format Base64 ne contient que des octets valides lors d'un transfert de contenu HTTP (ce qui peut ne pas être le cas avec le contenu binaire original).
        /// Comme il s'agit d'une transformation qui peut être longue, cette méthode vient avec le modèle async et un jeton de cancellation.
        /// Encode le résultat en UTF-8.
        /// Voir aussi:
        /// https://stackoverflow.com/questions/32702965/convert-a-very-large-binary-file-into-a-base64string-incrementally
        /// </summary>
        /// <param name="inFile">Fichier d'origine</param>
        /// <param name="outFile">Fichier final (si réussi). Va être écrasé si existant.</param>
        /// <param name="token">Jeton de cancellation (optionnel)</param>
        /// <param name="workDir">Chemin de travail. Si null, on prend le "temp dir" par défaut du système.</param>
        /// <returns>True si et seulement si la transformation s'est correctement effectuée et que le fichier sortant existe.</returns>
        [PublicAPI]
        public static async Task<bool> ConvertFiletoBase64(FileInfo inFile, FileInfo outFile, CancellationToken token, string workDir = null)
        {
            if (inFile == null || !inFile.Exists)
            {
                return false;
            }

            try
            {
                var tmpFile = DirectoryUtil.CreateLocalTemporaryFile(null, workDir);
                
                using (var outStream = File.Open(tmpFile, FileMode.Create))
                {
                    using (var cs = new CryptoStream(outStream, new ToBase64Transform(), CryptoStreamMode.Write))
                    {
                        using (var inStream = File.OpenRead(inFile.FullName))
                        {
                            await inStream.CopyToAsync(cs, 1024, token);
                        }
                    }
                }

                if (outFile.Exists) outFile.Delete();
                File.Move(tmpFile, outFile.FullName);
                outFile.Refresh();
            }
            catch (Exception e)
            {
                log.Error(e, "Error during Base64 conversion of file: " + inFile.FullName);
                return false;
            }

            return true;
        }

        public static async Task<bool> ConvertFileBackFromBase64(FileInfo inFile, FileInfo outFile, CancellationToken token, string workDir = null)
        {
            if (inFile == null || !inFile.Exists)
            {
                return false;
            }

            try
            {
                var tmpFile = DirectoryUtil.CreateLocalTemporaryFile(null, workDir);

                using (var f64 = File.Open(inFile.FullName, FileMode.Open))
                using (var cs = new CryptoStream(f64, new FromBase64Transform(), CryptoStreamMode.Read))
                using (var fo = File.Open(tmpFile, FileMode.Create))
                {
                    await cs.CopyToAsync(fo, 1024, token);
                }

                if (outFile.Exists) outFile.Delete();
                File.Move(tmpFile, outFile.FullName);
                outFile.Refresh();
            }
            catch (Exception e)
            {
                log.Error(e, "Error during from Base64 conversion of file: " + inFile.FullName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cette méthode charge une instance de fichier de configuration via son fichier par défaut dans le chemin
        /// par défaut de l'application présente.
        /// </summary>
        /// <param name="config">Une instance de la classe de configuration à instancier. Cette instance sera remplie par la lecture du fichier, ce qui peut écraser la configuration
        /// actuelle.</param>
        /// <param name="createDefaultConfig">Si le fichier par défaut n'existe pas, on tente de le créer via l'instance passée en paramètre.</param>
        /// <returns>TRUE si un fichier valide à été lu et l'instance en paramètre mis à jour. Faux sinon.</returns>
        public static bool LoadConfigFromDefaultFilepath([NotNull] IJsonConfig config, bool createDefaultConfig = true)
        {
            var fi = new FileInfo(DirectoryUtil.GetLocalApplicationDir() + config.DefaultConfigFileName);

            log.Trace("Try to load config file: {0}", fi.FullName);
            if (fi.Exists) return JsonUtil.UpdateFromJson(fi, config);

            if (!createDefaultConfig)
            {
                log.Warn("Configuration file does not exists and no default file is allowed.");
                return false;
            }

            var classType = config.GetType();
            var basicInstance = Activator.CreateInstance(classType);

            log.Trace("Creating default file: {0}", fi.FullName);
            if (!JsonUtil.WriteJsonFile(fi, basicInstance))
            {
                log.Error("Could not create default instance of type '{0}' to file '{1}'", classType.FullName, fi.FullName);
                return false;
            }

            fi.Refresh();
            if (fi.Exists) return JsonUtil.UpdateFromJson(fi, config);
            log.Error("File does not exists even after creation attempt.");
            return false;
        }
        
        /// <summary>
        /// Vérifie si un répertoire existe et que l'application peut écrire dedans.
        /// </summary>
        /// <param name="path">Le chemin complet à vérifier.</param>
        /// <returns>True s'il existe et que l'on peut y créer des fichier.</returns>
        public static bool CheckIfDirectoryIsWritable(string path)
        {
            if (!Directory.Exists(path)) return false;
            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, path);
            permissionSet.AddPermission(writePermission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }

        /// <summary>
        /// Cette méthode tente de trouver un fichier valide sur disque de facon absolue et, si incapable,
        /// à partir d'un ensemble valide de repertoires candidats.
        /// </summary>
        /// <param name="baseDirectories">La liste de sous-repertoire candidats.</param>
        /// <param name="file">Le nom de fichier visé.</param>
        /// <returns>Un object FileInfo ou null si aucun fichier n'est identifié sur disque.</returns>
        public static FileInfo FindFilePaths([NotNull] IEnumerable<DirectoryInfo> baseDirectories, [NotNull] string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return null;
            }

            // Test absolu
            if (File.Exists(file))
            {
                return new FileInfo(file);
            }

            foreach (var baseDir in baseDirectories)
            {
                var path = Path.Combine(baseDir.FullName, file);
                return File.Exists(path) ? new FileInfo(path) : null;
            }

            return null;
        }

        /// <summary>
        /// Cette méthode tente de trouver un fichier valide sur disque d'un chemin potentiel de fichier, et si incapable, selon un
        /// répertoire de base.
        /// </summary>
        /// <param name="baseDirectory">Le répertoire de base. Si null, ne fera que la vérification absolue.</param>
        /// <param name="file">Le chemin candidat du fichier.</param>
        /// <returns>Un FileInfo si un fichier existe. Null sinon.</returns>
        public static FileInfo FindFilePaths(DirectoryInfo baseDirectory, [NotNull] string file)
        {
            return FindFilePaths(baseDirectory?.FullName, file);
        }
        
        /// <summary>
        /// Cette méthode tente de trouver un fichier valide sur disque d'un chemin potentiel de fichier, et si incapable, selon un
        /// répertoire de base.
        /// </summary>
        /// <param name="baseDirectory">Le répertoire de base. Si null, ne fera que la vérification absolue.</param>
        /// <param name="file">Le chemin candidat du fichier.</param>
        /// <returns>Un FileInfo si un fichier existe. Null sinon.</returns>
        public static FileInfo FindFilePaths(string baseDirectory, [NotNull] string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return null;
            }

            // Test absolu
            if (File.Exists(file))
            {
                return new FileInfo(file);
            }

            // test avec répertoire de base.
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                return null;
            }
            var path = Path.Combine(baseDirectory, file);
            return File.Exists(path) ? new FileInfo(path) : null;
        }

        /// <summary>
        /// Cette méthode tente de trouver des fichier existant en testant un groupe de chemins potentiel.
        /// On tente d'abord de voir si le chemin est absolu, puis sinon, en ajoutant un répertoire de base.
        /// Quand on trouve un fichier valide, on le met dans un FileInfo valide. Sinon, null.
        /// Retourne un tableau de valeur qui est indexé de la même façon que les fichier candidats données en
        /// paramètres. Le retour est toujours non null, mais peut contenir zéro éléments si les paramères sont vides ou nul.
        /// </summary>
        /// <param name="baseDirectory">Le répertoire de base à ajouter si l'option absolue n'existe pas.</param>
        /// <param name="files">La liste de fichier à tester.</param>
        /// <returns>Un tableau non null avec le même nombre d'éléments que les paramètre d'entré. À chaque index, contient soit un FileInfo valide, ou null.</returns>
        public static FileInfo[] FindFilePaths(string baseDirectory, params string[] files)
        {
            if (files == null) return new FileInfo[0];
            var returnArray = new FileInfo[files.Length];

            for (var i = 0; i < files.Length; i++)
            {
                returnArray[i] = FindFilePaths(baseDirectory, files[i]);
            }

            return returnArray;
        }

        /// <summary>
        /// Cette méthode tente de trouver des fichier existant en testant un groupe de chemins potentiel.
        /// On tente d'abord de voir si le chemin est absolu, puis sinon, en ajoutant un répertoire de base.
        /// Quand on trouve un fichier valide, on le met dans un FileInfo valide. Sinon, null.
        /// Retourne un tableau de valeur qui est indexé de la même façon que les fichier candidats données en
        /// paramètres. Le retour est toujours non null, mais peut contenir zéro éléments si les paramères sont vides ou nul.
        /// </summary>
        /// <param name="baseDirectory">Le répertoire de base à ajouter si l'option absolue n'existe pas.</param>
        /// <param name="files">La liste de fichier à tester.</param>
        /// <returns>Un tableau non null avec le même nombre d'éléments que les paramètre d'entré. À chaque index, contient soit un FileInfo valide, ou null.</returns>
        public static FileInfo[] FindFilePaths(DirectoryInfo baseDirectory, params string[] files)
        {
            return FindFilePaths(baseDirectory?.FullName, files);
        }

        /// <summary>
        /// Trouve le premier fichier valide d'une série de fichier potentiels, en regardant d'abord la valeur absolue puis ajoutée d'un répertoire de base, si existant.
        /// Les candidats sont évalués dans l'ordre des paramètres données.
        /// </summary>
        /// <param name="baseDirectory">Répertoire de base. Optionel.</param>
        /// <param name="files">Les fichiers candidats à tester.</param>
        /// <returns>Le premier fichier existant sur disque. Null sinon.</returns>
        public static FileInfo FindFirstFilePath(string baseDirectory, params string[] files)
        {
            return files?.Select(f => FindFilePaths(baseDirectory, f)).FirstOrDefault(found => found != null);
        }

        /// <summary>
        /// Trouve le premier fichier valide d'une série de fichier potentiels, en regardant d'abord la valeur absolue puis ajoutée d'un répertoire de base, si existant.
        /// Les candidats sont évalués dans l'ordre des paramètres données.
        /// </summary>
        /// <param name="baseDirectory">Répertoire de base. Optionel.</param>
        /// <param name="files">Les fichiers candidats à tester.</param>
        /// <returns>Le premier fichier existant sur disque. Null sinon.</returns>
        public static FileInfo FindFirstFilePath(DirectoryInfo baseDirectory, params string[] files)
        {
            return FindFirstFilePath(baseDirectory?.FullName, files);
        }
        
        /// <summary>
        /// Génère un nom de fichier pour éviter à tout* prix une collision avec un nom de fichier existant.
        /// Utilise, pour mille tentatives, de créer un nom de fichier avec un numéro à la fin (comme Windows).
        /// Sinon, donne deux sels via GenerateUniqueId est conclu, même si après cela il y a encore une collision.
        /// Les chances que ça arrive sont astronomiquement faibles...
        /// </summary>
        /// <param name="fullName"> Le fichier à analyser. S'il n'existe pas, on ne fait rien.</param>
        /// <returns>Un nom de fichier potentiel, qui n'existe pas encore.</returns>
        public static FileInfo FileNameValidation(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return null;
            
            return FileNameValidation(new FileInfo(fullName));
        }

        /// <summary>
        /// Génère un nom de fichier pour éviter à tout* prix une collision avec un nom de fichier existant.
        /// Utilise, pour mille tentatives, de créer un nom de fichier avec un numéro à la fin (comme Windows).
        /// Sinon, donne deux sels via GenerateUniqueId est conclu, même si après cela il y a encore une collision.
        /// Les chances que ça arrive sont astronomiquement faibles...
        /// </summary>
        /// <param name="info"> Le fichier à analyser. S'il n'existe pas, on ne fait rien.</param>
        /// <returns>Un nom de fichier potentiel, qui n'existe pas encore. Peut être la même référence que l'argument si le fichier n'existe pas.</returns>
        public static FileInfo FileNameValidation([NotNull] FileInfo info)
        {
            if (!info.Exists) return info;

            var basePath = info.DirectoryName;
            var extension = info.Extension;
            var name = StringUtil.RemoveExtension(info.Name);

            return FileNameValidation(basePath, extension, name);
        }

        /// <summary>
        /// Génère un nom de fichier pour éviter à tout* prix une collision avec un nom de fichier existant.
        /// Utilise, pour mille tentatives, de créer un nom de fichier avec un numéro à la fin (comme Windows).
        /// Sinon, donne deux sels via GenerateUniqueId est conclu, même si après cela il y a encore une collision.
        /// Les chances que ça arrive sont astronomiquement faibles...
        /// </summary>
        /// <param name="basePath">Le chemin de base.</param>
        /// <param name="extension">L'extension du fichier (type de fichier).</param>
        /// <param name="nameNoExtension">Le nom du fichier, sans chemin, sans extension.</param>
        /// <returns>Un nom de fichier potentiel, qui n'existe pas encore.</returns>
        public static FileInfo FileNameValidation(string basePath, [NotNull] string extension, [NotNull] string nameNoExtension)
        {
            string testPath;
            var amount = 0;
            var dir = "";
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                dir = basePath;
                if (!dir.EndsWith(Path.DirectorySeparatorChar.ToString())) dir += Path.DirectorySeparatorChar;
            }

            do
            {
                var sb = new StringBuilder();
                //basePath + name + extension;
                sb.Append(dir);
                sb.Append(nameNoExtension);
                if (amount != 0)
                {
                    sb.Append('(');
                    sb.Append(amount);
                    sb.Append(')');
                }
                sb.Append(extension);
                testPath = sb.ToString();
                amount++;
            } while (File.Exists(testPath) && amount < 1000);

            // Après mille tentative, de la marde...
            if (amount >= 1000)
            {
                testPath = basePath + nameNoExtension + "_" + GenerateUniqueId.GetId() + "_" + GenerateUniqueId.GetId() + extension;
            }

            return new FileInfo(testPath);
        }

        /// <summary>
        /// S'assure qu'un transfert de fichier d'une source à une destination est faite avec toutes les
        /// vérifications nécessaire pour déterminer si l'opération est un succès.
        /// 1- Le fichier original existe.
        /// 2- Si le fichier destination existe, il est détruit puis remplacé.
        /// 3- À la toute fin, l'opération est un succès si le fichier destination existe.
        /// </summary>
        /// <param name="srcPath">Le fichier source.</param>
        /// <param name="destPath">Le fichier destination.</param>
        /// <returns>TRUE si toutes les opérations et vérification retournent vrai.</returns>
        public static bool CompleteMove([NotNull] string srcPath, [NotNull] string destPath)
        {
            if (!File.Exists(srcPath))
            {
                return false;
            }

            try
            {
                var dirDestination = Path.GetDirectoryName(destPath);
                if (Directory.Exists(dirDestination))
                {
                    var dir = Directory.CreateDirectory(dirDestination);
                    if (!dir.Exists)
                    {
                        log.Warn("Could not create directory structure for destination file: " + destPath);
                        return false;
                    }
                }

                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }

                File.Move(srcPath, destPath);
                
                return File.Exists(destPath);
            }
            catch (FileNotFoundException e)
            {
                log.Error(e,"File not found exception for the following destination: "+ destPath);
                return false;
            }
        }

        /// <summary>
        /// Méthode pour regarder si on a accès à un fichier en lecture. Parfois, il faut attendre un certain temps avant d'avoir accès à une ressource.
        /// Peut ne pas être utile dans le cas d'un "race-condition" par contre, mais si on est raisonablement sûr qu'un seul fil d'exécution va tenter de lire
        /// le fichier, on peut utiliser cette méthode.
        /// Un fichier invalide retourne faux.
        /// Voir ici pour plus d'information: https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
        /// </summary>
        /// <param name="fileToCheck">Le fichier à vérifier.</param>
        /// <param name="tries">Le nombre d'essais à faire.</param>
        /// <param name="waitMili">Le temps d'attente entre chaque essais. Ce temps sera également utilisé pour distancer la vérification de cette méthode.</param>
        /// <returns>TRUE si on a réussi à avoir une lecture dans le nombre d'essais requis.</returns>
        public static async Task<bool> CheckAndWaitForFileLock([NotNull] FileInfo fileToCheck, uint tries = 5, uint waitMili = 100)
        {
            for (var nbr = 0; nbr < tries; nbr++)
            {

                try
                {
                    fileToCheck.Refresh();
                    if (!fileToCheck.Exists) return false;

                    using (new FileStream(fileToCheck.FullName, FileMode.Open))
                    {
                        // Pas de code, on veut juste voir si on a accès
                    }
                    await Task.Delay((int)waitMili);
                    return true;
                }
                catch (Exception)
                {
                    // Die-nice
                    log.Trace("File is being locked. Try nbr: {0}, File => {1}", nbr+1, fileToCheck.FullName);
                    await Task.Delay((int)waitMili);
                }
            }

            return false;
        }

        /// <summary>
        /// Comparaison binaire complète de deux fichiers. Plus rapide que de faire un HASH, car on ne fait pas la fonction de hash.
        /// Voir: https://docs.microsoft.com/en-us/troubleshoot/dotnet/csharp/create-file-compare
        /// </summary>
        /// <returns>TRUE si et seulement si les fichiers existent, et sont identiques au niveau binaire.</returns>
        public static bool BinaryFileEqualCheck(string file1, string file2)
        {
            if (!File.Exists(file1) || !File.Exists(file2))
            {
                return false;
            }

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            using (FileStream fs1 = new FileStream(file1, FileMode.Open), fs2 = new FileStream(file2, FileMode.Open))
            {

                // Check the file sizes. If they are not the same, the files
                // are not the same.
                if (fs1.Length != fs2.Length)
                {
                    // Return false to indicate files are different
                    return false;
                }

                // Read and compare a byte from each file until either a
                // non-matching set of bytes is found or until the end of
                // file1 is reached.
                int file1Byte;
                int file2Byte;
                do
                {
                    // Read one byte from each file.
                    file1Byte = fs1.ReadByte();
                    file2Byte = fs2.ReadByte();
                } while ((file1Byte == file2Byte) && (file1Byte != -1));
                
                // Return the success of the comparison. "file1byte" is
                // equal to "file2byte" at this point only if the files are
                // the same.
                return ((file1Byte - file2Byte) == 0);
            }
        }

        /// <summary>
        /// Méthode pour détecté que deux collections contiennent les même fichiers, suite à une extraction ou une copie.
        /// 
        /// Compare deux collections de fichiers ensemble pour vérifier que les fichiers sont "pareils", avec les détails suivants:
        /// - Les deux collections doivent contenir le même nombre d'éléments, ou références à des fichiers.
        /// - Chaque fichier doit avoir un fichier avec le même nom dans l'autre liste: son "jumeau".
        /// - Ils doivent venir de deux répertoires différents (donc ne sont pas des références vers le même fichier)
        /// - Chaque fichier doit avoir un jumeau avec le même nombre de bytes de taille de fichier.
        ///
        /// Par contre:
        /// - Chaque jumeau est évalué sur une base individuelle: ils peuvent donc venir de plusieurs répertoires différents (plus que deux), du moment que les jumeaux soient présents.
        /// - Attention par contre que les noms de fichiers soient uniques à l'intérieur de chaque groupe!
        /// - On ne fait pas de validation de "hashing".
        /// - Les dates de modifications et de création ne sont pas vérifiées.
        /// </summary>
        /// <param name="filesA"></param>
        /// <param name="filesB"></param>
        /// <returns>TRUS si les deux collections contiennent sont des copies fonctionnelles des même fichiers.</returns>
        public static bool CompareFileListsShallow([NotNull] IEnumerable<FileInfo> filesA, [NotNull] IEnumerable<FileInfo> filesB)
        {
            var dicoA = filesA.ToDictionary(f => f.Name);
            var dicoB = filesB.ToDictionary(f => f.Name);

            if (dicoA.Count != dicoB.Count)
            {
                log.Debug($"Collections do not have the same size: {dicoA.Count} != {dicoB.Count}");
                return false;
            }

            using (var iter = dicoA.GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    var cur = iter.Current;

                    // On regarde la correspondance de nom.
                    if (!dicoB.TryGetValue(cur.Key, out var valB))
                    {
                        log.Debug("Key has no corresponding twin: " + cur.Key);
                        return false;
                    }

                    var valA = cur.Value;

                    // On vérifie les critères d'égalité.
                    if (valA.Length != valB.Length) {
                        log.Debug($"Different file size:\n\t- {valA.FullName} => {valA.Length} bytes\n\t- {valB.FullName} => {valB.Length} bytes");
                        return false; // Ont des tailles différentes.
                    }
                    if (valA.FullName.Equals(valB.FullName)) {
                        log.Debug($"Same file on disk: {valA.FullName}");
                        return false; // Est pas le même fichier sur disque.
                    }
                }
            }

            return true;
        }
    }
}

