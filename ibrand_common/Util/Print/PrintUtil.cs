using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.Common.Util.Print
{
    /// <summary>
    /// Méthodes utilitaires pour l'impression.
    /// </summary>
    [PublicAPI]
    public static class PrintUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Localisation par défaut du fichier de configuration local de l'impression.
        /// </summary>
        public static readonly FileInfo DEFAULT_PRINTER_CONFIG_FILE = new FileInfo(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) + "printer.config");

        /// <summary>
        /// Localisation par défaut du fichier de configuration local de l'impression.
        /// </summary>
        public static readonly FileInfo DEFAULT_PRINTER_COUNT_FILE = new FileInfo(DirectoryUtil.GetLocalApplicationDir(DirectoryUtil.ApplicationNameEnum.Nomad) + "printer_count.json");

        /// <summary>
        /// Tente de faire une impression de façon synchrone.
        /// </summary>
        /// <param name="filePath">Le fichier à imprimer.</param>
        /// <param name="printInfo">L'information d'impression. Si null, on va chercher celle sur disque. Si elle
        /// n'existe pas non plus, on retourne immédiatement false.</param>
        /// <param name="windowControl">Référence vers le contrôle de Form qui a invoqué la méthode. Si non-nul, on teste si on doit avertir l'usager d'un
        /// manque de papier potentiel.</param>
        /// <returns>TRUE si l'impression à été envoyée à l'imprimante.</returns>
        public static bool Print(string filePath, PrintJobInfo printInfo = null, Control windowControl = null)
        {
            var config = printInfo ?? JsonUtil.ReadJsonFile<PrintJobInfo>(DEFAULT_PRINTER_CONFIG_FILE);

            if (config == null)
            {
                return false;
            }

            var pj = CreatePrintJob(config);
            if (pj == null)
                return false;
            log.Trace("Print job object created: {0}", pj);

            // Impression ok, on augmente le compteur;
            var count = File.Exists(DEFAULT_PRINTER_COUNT_FILE.FullName) ?
                JsonUtil.ReadJsonFile<PrintCountInfo>(DEFAULT_PRINTER_COUNT_FILE) : new PrintCountInfo();
            count.PrintActionCounter++;
            JsonUtil.WriteJsonFile(DEFAULT_PRINTER_COUNT_FILE, count);
            log.Trace("Print count incremented.");

            // Vérification de l'existence d'un contrôle pour valider si on avertit l'usager d'un possible manque de papier.
            if (windowControl != null && count.PrintActionCounterShowWarning) {
                if (count.PrintActionCounter >= count.PrintActionCounterThreshold) {
                    Task.Run(() => {
                        windowControl.SafeCall(() => {
                            MsgBox.Show(windowControl, Resources.common.UI_PrintUtil_RollLow, Resources.common.app_title, MessageBoxButtons.OK);
                        });
                    });
                }
            }

            pj.AddActionDrawBitmap(filePath);
            log.Trace("Added Print job file reference.");

            var rez = pj.DoWork(null);

            log.Trace("Print job done.");

            return rez;
        }

        /// <summary>
        /// Obtient la liste des imprimantes sur la machine.
        /// </summary>
        /// <returns>Une liste de noms des imprimantes.</returns>
        public static IEnumerable<string> GetPrinterNamesList()
        {
            IEnumerable<string> list;

            try
            {
                list = PrinterSettings.InstalledPrinters.Cast<string>();
            }
            catch (Exception e)
            {
                log.Fatal(e, "Machine unable to fetch it's printer name list. Correct this problem or switch machines to backup. Check if the following services are automatic and started using the 'services.msc' executable:\n\t'Print Spooler', 'Remote Procedure Call (RPC)', 'Remote Procedure Call (RPC) Locator'");
                list = new List<string>();
            }

            return list;
        }

        /// <summary>
        /// Retourne le nom de l'imprimante par défaut courante tel qu'identifié par le système.
        /// </summary>
        /// <returns>Le nom de l'imprimante à utiliser.</returns>
        public static string GetCurrentDefaultPrinterName()
        {
            try
            {
                var printerName = GetPrinterNamesList().FirstOrDefault(printer =>
                {
                    var test = new PrinterSettings { PrinterName = printer };
                    return test.IsDefaultPrinter;
                });
                return printerName;
            }
            catch (Exception e)
            {
                log.Error(e, "Error during default printer search.");
                return null;
            }
        }

        /// <summary>
        /// Créer une instance de la classe PrintJob, avec deux options de création de configuration par défaut.
        /// On peut par la suite s'abonner aux événement de cette instance, et on peut la réutiliser si on veut garder les mêmes
        /// configurations d'impression.
        /// L'instance n'est pas prête immédiatement à imprimer; il faut lui donner des paramètres d'impression de pages avant.
        /// Si le nom d'imprimante donnée est invalide, lance une exception.
        /// </summary>
        /// <param name="printerName">Le nom de l'imprimante. Doit absolument correspondre à une imprimante valide au moment de la
        /// création.</param>
        /// <param name="useSystemDefault">Indique si on veut les configuration par défaut du système. FALSE par défaut.</param>
        /// <param name="printJobName">Le nom de la "job" dans le "spool". 'iBrand' par défaut.</param>
        /// <returns>Une référence vers l'instance de PrintJob créée.</returns>
        public static PrintJob CreatePrintJob(string printerName, bool useSystemDefault = false, string printJobName = "iBrand")
        {
            if (!useSystemDefault)
            {
                return CreatePrintJob(printerName, null, null, null, null, null, null, null, printJobName);
            }

            var ps = new PrinterSettings
            {
                PrinterName = printerName // Faire cela provoque un chargement des propriétés de l'imprimante.
            };

            var printDoc = new PrintDocument
            {
                PrinterSettings = ps,
                OriginAtMargins = true, // Very mucho important!
                DefaultPageSettings = ps.DefaultPageSettings,
                DocumentName = printJobName,
                PrintController = new StandardPrintController() // enlève le dialogue de status d'impression.
            };

            var printJob = new PrintJob(ps, ps.DefaultPageSettings, printDoc);

            return printJob;
        }

        /// <summary>
        /// Créer une instance de la classe PrintJob à partir d'une foule d'options.
        /// On peut par la suite s'abonner aux événement de cette instance, et on peut la réutiliser si on veut garder les mêmes
        /// configurations d'impression.
        /// L'instance n'est pas prête immédiatement à imprimer; il faut lui donner des paramètres d'impression de pages avant.
        /// Si le nom d'imprimante donnée est invalide, lance une exception.
        /// </summary>
        /// <param name="printerName">Le nom de l'imprimante. Doit absolument correspondre à une imprimante valide au moment de la
        /// création.</param>
        /// <param name="nbrCopies">Le nombre de copie voulue. Optionnel. Défaut = 1</param>
        /// <param name="horizontalDpi">Le nombre de "Dot per Inch" horizontal. Optionnel. Défaut = 300</param>
        /// <param name="verticalDpi">Le nombre de "Dot per Inch" vertical. Optionnel. Défaut = 300</param>
        /// <param name="landscape">Indique si on veut imprimer en orientation paysage. Optionnel. Défaut = false</param>
        /// <param name="margins">Les marges sur le côté de l'impression. Optionnel. Défaut = (0,0,0,0), soit aucune marge.</param>
        /// <param name="paperSize">La taille de papier.  Optionnel. Défaut = 8.5x11 pouces (format lettre)</param>
        /// <param name="greyscale">Indique si on imprime en noir et blanc. Optionnel. Défaut = false</param>
        /// <param name="printJobName">Le nom du document d'impression dans le spool. Optionnel. Défaut = "iBrand"</param>
        /// <returns>Une référence vers l'instance de PrintJob créée.</returns>
        public static PrintJob CreatePrintJob(string printerName, short? nbrCopies, int? horizontalDpi, int? verticalDpi, 
            bool? landscape, Margins margins, PaperSize paperSize, bool? greyscale, string printJobName)
        {
            /* Voir:
             * http://stackoverflow.com/questions/296182/how-to-get-printer-info-in-net
             * https://msdn.microsoft.com/en-us/library/system.drawing.printing.printdocument%28v=vs.90%29.aspx
             * https://msdn.microsoft.com/en-us/library/system.drawing.printing.printdocument.originatmargins%28v=vs.110%29.aspx
             */

            var ps = new PrinterSettings
            {
                PrinterName = printerName, // Faire cela provoque un chargement des propriétés de l'imprimante.
                Copies = nbrCopies ?? 1
            };

            if (!ps.IsValid)
            {
                throw new Exception("Invalid printer. Does not exists in the current printer list.");
            }
            
            var pageSetting = new PageSettings(ps)
            {
                PrinterResolution = new PrinterResolution
                {
                    X = horizontalDpi ?? 300,
                    Y = verticalDpi ?? 300
                },
                Landscape = landscape ?? false,
                Margins = margins ?? new Margins
                {
                    Left = 0,
                    Top = 0,
                    Right = 0,
                    Bottom = 0
                },
                PaperSize = paperSize ?? new PaperSize
                {
                    Width = (int)(8.5 * 100),
                    Height = (int)(11.0 * 100),
                    PaperName = "(8.5x11)"
                },
                Color = !(greyscale ?? true)
            };
            var printDoc = new PrintDocument
            {
                PrinterSettings = ps,
                OriginAtMargins = true, // Very mucho important!
                DefaultPageSettings = pageSetting,
                DocumentName = printJobName ?? "iBrand",
                PrintController = new StandardPrintController() // enlève le dialogue de status d'impression.
            };

            var printJob = new PrintJob(ps, pageSetting, printDoc);

            return printJob;
        }
        
        /// <summary>
        /// Transforme en chaîne de caractère les configurations d'une imprimante.
        /// </summary>
        /// <param name="pSettings">Les configurations de l'imprimante.</param>
        /// <param name="tabLevel">Le nombre de tabulation de base à utiliser.</param>
        /// <returns>La chaîne de caractère lisible, avec indentations.</returns>
        public static string ToString(PrinterSettings pSettings, int tabLevel = 0)
        {
            if (pSettings == null) return "null";

            // On créé le préfixe de tabulation
            var prefixeSb = new StringBuilder();
            for (var i = 0; i < tabLevel; i++)
            {
                prefixeSb.Append('\t');
            }
            var prefixe = prefixeSb.ToString();

            var sb = new StringBuilder();

            sb.AppendLine(prefixe + $"Printer settings for printer {pSettings.PrinterName}");
            sb.AppendLine(prefixe + $"-Can duplex: {pSettings.CanDuplex}");
            sb.AppendLine(prefixe + $"-Duplex mode: {pSettings.Duplex}");
            sb.AppendLine(prefixe + $"-Is default printer: {pSettings.IsDefaultPrinter}");
            sb.AppendLine(prefixe + $"-Is plotter: {pSettings.IsPlotter}");
            sb.AppendLine(prefixe + $"-Is valid: {pSettings.IsValid}");
            sb.AppendLine(prefixe + $"-Landscape angle: {pSettings.LandscapeAngle}");
            sb.AppendLine(prefixe + $"-Copies: {pSettings.Copies}");
            sb.AppendLine(prefixe + $"-Is supporting colors: {pSettings.SupportsColor}");
            sb.AppendLine(prefixe + $"-Collate: {pSettings.Collate}");
            sb.AppendLine(prefixe + "-Default page settings: ");
            sb.AppendLine(ToString(pSettings.DefaultPageSettings, tabLevel + 1));
            sb.AppendLine(prefixe + "-Paper sizes: ");
            sb.AppendLine(ToString(pSettings.PaperSizes, tabLevel + 1));
            sb.AppendLine(prefixe + "-Paper sources: ");
            sb.AppendLine(ToString(pSettings.PaperSources, tabLevel + 1));
            sb.AppendLine(prefixe + "-Printer resolutions: ");
            sb.AppendLine(ToString(pSettings.PrinterResolutions, tabLevel + 1));
            
            return sb.ToString();
        }

        /// <summary>
        /// Construit une chaîne de caractère pour illustrer de façon verticale le contenu d'une énumération
        /// </summary>
        /// <param name="collection">La collection d'items</param>
        /// <param name="tabLevel">Le nombre de tabulation de base à utiliser.</param>
        /// <returns>La chaîne de caractère lisible, avec indentations.</returns>
        public static string ToString(ICollection collection, int tabLevel = 0)
        {
            if (collection == null) return "null";

            // On créé le préfixe de tabulation
            var prefixeSb = new StringBuilder();
            for (var i = 0; i < tabLevel; i++)
            {
                prefixeSb.Append('\t');
            }
            var prefixe = prefixeSb.ToString();

            var sb = new StringBuilder();

            foreach (var item in collection)
            {
                sb.Append(prefixe);
                sb.AppendLine(item.ToString());
            }
            
            return sb.ToString();
        }

        /// <summary>
        /// Transforme en chaîne de caractère les configurations d'une page à imprimer.
        /// </summary>
        /// <param name="pSettings">Les configurations de la page.</param>
        /// <param name="tabLevel">Le nombre de tabulation de base à utiliser.</param>
        /// <returns>La chaîne de caractère lisible, avec indentations.</returns>
        public static string ToString(PageSettings pSettings, int tabLevel = 0)
        {
            if (pSettings == null) return "null";

            // On créé le préfixe de tabulation
            var prefixeSb = new StringBuilder();
            for (var i = 0; i < tabLevel; i++)
            {
                prefixeSb.Append('\t');
            }
            var prefixe = prefixeSb.ToString();

            var sb = new StringBuilder();

            sb.AppendLine(prefixe + $"-Bounds: {pSettings.Bounds}");
            sb.AppendLine(prefixe + $"-In color: {pSettings.Color}");
            sb.AppendLine(prefixe + $"-Hard margins: ({pSettings.HardMarginX}, {pSettings.HardMarginY})");
            sb.AppendLine(prefixe + $"-Landscape: {pSettings.Landscape}");
            sb.AppendLine(prefixe + $"-Margins: {pSettings.Margins}");
            sb.AppendLine(prefixe + $"-Paper size: {pSettings.PaperSize}");
            sb.AppendLine(prefixe + $"-Paper source: {pSettings.PaperSource}");
            sb.AppendLine(prefixe + $"-Resolution: {pSettings.PrinterResolution}");
            sb.AppendLine(prefixe + $"-Printable area: {pSettings.PrintableArea}");

            return sb.ToString();
        }

        /// <summary>
        /// Trouve le premier type de papier dont le nom correspond à ce qui est donné.
        /// On fait une recherche dans une configuration d'imprimante, et on peut donner soit un nom exact, ou une partie
        /// de nom. On retourne le premier type de papier trouvé.
        /// Si on ne trouve rien ou si les configurations sont nulles, on retourne nul.
        /// </summary>
        /// <param name="pSettings">Les settings de l'imprimantes.</param>
        /// <param name="paperNameCode">Le nom du papier voulu.</param>
        /// <param name="exactName">Indique si on cherche le nom exact du papier. FALSE par défaut.</param>
        /// <returns>La référence vers le code de papier trouvé.</returns>
        public static PaperSize FindPaperSizeFromPrinterSettings(PrinterSettings pSettings, string paperNameCode, bool exactName = false)
        {
            if (pSettings == null || pSettings.PaperSizes.Count == 0) return null;

            PaperSize paper = null;

            var col = pSettings.PaperSizes;
            for (var i = 0; i < col.Count; i++)
            {
                if (exactName)
                {
                    if (!col[i].PaperName.Equals(paperNameCode, StringComparison.InvariantCulture)) continue;
                    paper = col[i];
                    break;
                }

                if (!col[i].PaperName.Contains(paperNameCode)) continue;
                paper = col[i];
                break;
            }

            return paper;
        }

        /// <summary>
        /// Vérifie si une configuration de papier précise existe dans les configurations disponibles d'une imprimante.
        /// </summary>
        /// <param name="pSettings">Configuration d'imprimante.</param>
        /// <param name="paper">Le type de papier à trouver.</param>
        /// <returns>TRUE si la configuration contient le type de papier à trouver.</returns>
        public static bool FindPaperSizeFromPrinterSettings(PrinterSettings pSettings, PaperSize paper)
        {
            if (pSettings == null || pSettings.PaperSizes.Count == 0 || paper  == null) return false;
            
            var col = pSettings.PaperSizes;
            for (var i = 0; i < col.Count; i++)
            {
                if (!Equals(paper, col[i])) continue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Examine les imprimantes détectées par le système et détermine si le nom d'imprimante donnée en paramètre existe ou non.
        /// Si oui, on retourne une référence de PrinterSettings de base lié à cette imprimante. On vérifie aussi si cette imprimante est valide,
        /// c'est-à-dire qu'elle est capable d'impression, et non juste une imprimante offline.
        /// </summary>
        /// <param name="printerName">Le nom de l'imprimante.</param>
        /// <param name="exactName">Si oui, il faut que le nom donné en paramètre soit rigoureusement identique à un nom
        /// d'imprimante sur le système.</param>
        /// <returns>Si une imprimante valide est détectée, on retourne une référence non-nulle de configuration de base.</returns>
        public static PrinterSettings FindPrinterSettingsFromSystemList(string printerName, bool exactName = false)
        {
            if (printerName == null) return null;

            PrinterSettings settings = null;

            var printerList = GetPrinterNamesList();
            foreach (var printer in printerList)
            {
                if (exactName)
                {
                    if (!printerName.Equals(printer, StringComparison.InvariantCulture)) continue;
                    var tmp = new PrinterSettings {PrinterName = printer};
                    if (tmp.IsValid)
                    {
                        settings = tmp;
                        break;
                    }
                }

                if (!printerName.Contains(printer)) continue;
                var tmp2 = new PrinterSettings { PrinterName = printer };
                if (!tmp2.IsValid) continue;
                settings = tmp2;
                break;
            }

            return settings;
        }

        /// <summary>
        /// Créer une référence vers un job d'impression complète avec les configurations donnés par un objet issus d'un fichier de configuration.
        /// </summary>
        /// <param name="info">Les informations pour recréer la tâche d'impression. Il faut quand même invoquer les méthode d'ajout de
        /// fichiers d'impression avec les options de dessein pour préparer l'impression.</param>
        /// <returns>Une référence vers le job d'impression. S'il n'est pas possible de créer le job d'impression avec les configurations données,
        /// on retourne null.</returns>
        public static PrintJob CreatePrintJob(PrintJobInfo info)
        {
            if (info == null) return null;

            var ps = FindPrinterSettingsFromSystemList(info.PrinterName, true);
            if (ps == null) return null;

            var paper = FindPaperSizeFromPrinterSettings(ps, info.PaperSize);
            if (!paper) return null;

            ps.DefaultPageSettings.PaperSize = info.PaperSize;

            var printDoc = new PrintDocument
            {
                PrinterSettings = ps,
                OriginAtMargins = true, // Very mucho important!
                DefaultPageSettings = ps.DefaultPageSettings,
                DocumentName = "iBrand",
                PrintController = new StandardPrintController() // enlève le dialogue de status d'impression.
            };

            return new PrintJob(ps, ps.DefaultPageSettings, printDoc, info.DrawOptions);
        }

        /// <summary>
        /// Comme l'objet PrinterSettings ne possède pas un comparateur d'égalité explicite, il faut en faire un.
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <returns>Égalité</returns>
        public static bool Equals(PrinterSettings arg1, PrinterSettings arg2)
        {
            if (arg1 == null && arg2 == null) return true;
            if (arg1 == null || arg2 == null) return false;
            return Equals(arg1.DefaultPageSettings, arg2.DefaultPageSettings) &&
                   arg1.CanDuplex == arg2.CanDuplex &&
                   arg1.IsPlotter == arg2.IsPlotter &&
                   arg1.SupportsColor == arg2.SupportsColor &&
                   arg1.LandscapeAngle == arg2.LandscapeAngle &&
                   arg1.PrinterName.Equals(arg2.PrinterName);
        }

        /// <summary>
        /// Comme l'objet PageSettings ne possède pas un comparateur d'égalité explicite, il faut en faire un.
        /// </summary>
        /// <param name="arg1">Argument 1</param>
        /// <param name="arg2">Argument 2</param>
        /// <returns>Égalité</returns>
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static bool Equals(PageSettings arg1, PageSettings arg2)
        {
            if (arg1 == null && arg2 == null) return true;
            if (arg1 == null || arg2 == null) return false;
            return arg1.Landscape == arg2.Landscape &&
                   Equals(arg1.PaperSize, arg2.PaperSize) &&
                   arg1.HardMarginX == arg2.HardMarginX &&
                   arg1.HardMarginY == arg2.HardMarginY &&
                   Equals(arg1.Margins, arg2.Margins) &&
                   Equals(arg1.Bounds, arg2.Bounds) &&
                   Equals(arg1.PrintableArea, arg2.PrintableArea) &&
                   Equals(arg1.PaperSource, arg2.PaperSource);
        }

        /// <summary>
        /// Comme il n'existe pas de comparateur pour PaperSize, on en créé un.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns>Égalité</returns>
        public static bool Equals(PaperSize arg1, PaperSize arg2)
        {
            if (arg1 == null && arg2 == null) return true;
            if (arg1 == null || arg2 == null) return false;
            return arg1.Width == arg2.Width &&
                   arg1.Height == arg2.Height &&
                   arg1.PaperName.Equals(arg2.PaperName) &&
                   arg1.Kind == arg2.Kind &&
                   arg1.RawKind == arg2.RawKind;
        }

        /// <summary>
        /// Comme il n'existe pas de comparateur pour Margins, on en créé un.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns>Égalité</returns>
        public static bool Equals(Margins arg1, Margins arg2)
        {
            if (arg1 == null && arg2 == null) return true;
            if (arg1 == null || arg2 == null) return false;
            return arg1.Bottom == arg2.Bottom &&
                   arg1.Left == arg2.Left &&
                   arg1.Right == arg2.Right &&
                   arg1.Top == arg2.Top;
        }

        /// <summary>
        /// Comme il n'existe pas de comparateur pour PaperSource, on en créé un.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns>Égalité</returns>
        public static bool Equals(PaperSource arg1, PaperSource arg2)
        {
            if (arg1 == null && arg2 == null) return true;
            if (arg1 == null || arg2 == null) return false;
            return arg1.RawKind == arg2.RawKind &&
                   Equals(arg1.SourceName, arg2.SourceName) &&
                   Equals(arg1.Kind, arg2.Kind);
        }

        /// <summary>
        /// Fait une vérification que l'on possède une configuration locale d'impression, et qu'il est donc possible
        /// de lancer une impression en tant que service sur une application locale. Tente de valider que l'imprimante est également accessible,
        /// sans lancer d'impression test.
        /// </summary>
        /// <returns>TRUE si et seulement si il existe une configuration locale d'imprimante, et que l'imprimante en question
        /// soit disponible.</returns>
        public static bool IsPrintReady()
        {
            var config = JsonUtil.ReadJsonFile<PrintJobInfo>(DEFAULT_PRINTER_CONFIG_FILE);

            if (config == null)
            {
                return false;
            }

            var settings = FindPrinterSettingsFromSystemList(config.PrinterName, true);

            return settings != null && settings.IsValid;
        }
    }
}
