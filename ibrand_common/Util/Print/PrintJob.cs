using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NLog;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.Common.Util.Print
{
    /// <summary>
    /// Cette classe encapsule le processus d'invocation et de gestion des événements pour l'impression d'un document complexe.
    /// Elle permet de contrôler l'ordre d'impression via des actions personnalisée, placées dans une pile d'appels. On peut donc invoquer cette
    /// classe avec des méthodes externes faites sur mesure pour chaque page, à l'aide de l'objet "PrintPageEventArgs".
    /// Il n'est pas possible de créer cette méthode directement, pour protéger les objets de gestion d'impression. On peut
    /// par contre s'abonner aux divers événements pour voir la progression et le résultat de l'impression.
    /// Il faut utiliser la classe "PrintUtil" pour créer une instance de cette classe.
    /// On peut lancer l'impression de façon synchrone ou asynchrone, selon la méthode d'appel via la méthode "DoWork".
    /// Il est entièrement possible de réutiliser la même instance configurer entre deux "job" d'impression pour imprimer avec les
    /// mêmes paramètres, mais pas les même pages.
    /// </summary>
    public class PrintJob : IEquatable<PrintJob>
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly List<Action<PrintPageEventArgs>> _pageDrawActions = new List<Action<PrintPageEventArgs>>();
        private int _currentPage;
        private CancellationToken? _ct;
        private bool _lastPrintStatus;
        private PrintAction _printAction;
        private const float DEFAULT_RESOLUTION = 300f;

        /// <summary>
        /// Les options de dessein d'une image à utiliser pour chaque image, à moins que les méthodes de création ne passent
        /// explicitement d'autres options. Ces options ne sont vérifiées qu'au moment de l'ajout d'une image.
        /// </summary>
        public PrintJobImageOption DefaultImageOption { get; set; }

        /// <summary>
        /// Indique que la page X a été imprimée. 
        /// </summary>
        public event EventHandler<int> PagePrinted;

        /// <summary>
        /// Indique que l'impression à commencée.
        /// </summary>
        public event EventHandler PrintStarted;

        /// <summary>
        /// Indique quand l'impression est terminée, et si l'impression s'est bien déroulée.
        /// </summary>
        public event EventHandler<bool> PrintFinished;

        /// <summary>
        /// Constructeur interne. Est fait ainsi pour être créer par la classe PrintUtil.
        /// </summary>
        /// <param name="printSettings">Les paramètres de l'imprimante.</param>
        /// <param name="pageSettings">Les paramètres de chaque page.</param>
        /// <param name="printDocument">Les paramètres globaux de l'impression.</param>
        /// <param name="defaultImageOptions">Options de dessein d'une image par défaut.</param>
        internal PrintJob(PrinterSettings printSettings, PageSettings pageSettings, PrintDocument printDocument, PrintJobImageOption defaultImageOptions = null)
        {
            PrinterSettings = printSettings;
            PageSettings = pageSettings;
            PrintDocument = printDocument;
            DefaultImageOption = defaultImageOptions ?? new PrintJobImageOption();

            printDocument.BeginPrint += BeginPrintDo;
            printDocument.EndPrint += EndPrintDo;
            printDocument.PrintPage += PrintPageDo;
        }

        /// <summary>
        /// Assigne la liste d'action actuelle par une collection d'actions.
        /// Copie de surface.
        /// Lance un exception si on est en train d'imprimer.
        /// </summary>
        /// <param name="actions">La collection d'actions à exécuter.</param>
        public void SetActionsDraw(IEnumerable<Action<PrintPageEventArgs>> actions)
        {
            if (IsPrinting) throw new InvalidOperationException("Cannot modify print actions during impression.");

            _pageDrawActions.Clear();
            _pageDrawActions.AddRange(actions);
        }

        /// <summary>
        /// Ajoute une action de page d'impression à la liste déjà existante.
        /// Lance une exception si on est en train d'imprimer.
        /// </summary>
        /// <param name="action">L'action à ajouter.</param>
        public void AddActionDraw(Action<PrintPageEventArgs> action)
        {
            if (IsPrinting) throw new InvalidOperationException("Cannot modify print actions during impression.");
            _pageDrawActions.Add(action);
        }

        /// <summary>
        /// Assigne une liste d'images à écrire directement pour chaque page du document.
        /// Lance une exception si on est en train d'imprimer.
        /// </summary>
        /// <param name="bitmaps">La collection d'images à imprimer.</param>
        /// <param name="drawOptions">Options d'impressions. Nul par défaut.</param>
        public void SetActionsDrawBitmaps(IEnumerable<Bitmap> bitmaps, PrintJobImageOption drawOptions = null)
        {
            if (IsPrinting) throw new InvalidOperationException("Cannot modify print actions during impression.");

            _pageDrawActions.Clear();
            foreach (var bitmap in bitmaps)
            {
                AddActionDrawBitmap(bitmap, drawOptions);
            }
        }

        /// <summary>
        /// Ajoute un image à imprimer à la liste déjà existante.
        /// Lance une exception si on est en train d'imprimer.
        /// </summary>
        /// <param name="bmp">L'image à ajouter.</param>
        /// <param name="drawOptions">Options d'impressions</param>
        public void AddActionDrawBitmap(Bitmap bmp, PrintJobImageOption drawOptions = null)
        {
            if (IsPrinting) throw new InvalidOperationException("Cannot modify print actions during impression.");

            if (drawOptions == null)
                drawOptions = DefaultImageOption ?? new PrintJobImageOption();

            // Comme l'envoi d'impression est asynchrone, on s'assure que la référence des options ne peut être modifiée entre
            // maintenant et l'impression elle-même.
            var copy = drawOptions.Clone();

            _pageDrawActions.Add(e =>
            {
                DrawPrintBitmap(bmp, e, copy);
            });
        }

        /// <summary>
        /// Indique si l'on veut ajouter un rectangle pour identifier la zone où il est impossible d'imprimer.
        /// Ne fonctionne qu'avec les actions générés par cette instances. En l'occurence, "AddActionDrawBitmap".
        /// </summary>
        public bool DebugPrintAreaForInstanceBitmaps { get; set; } = false;

        // ReSharper disable once SuggestBaseTypeForParameter
        private void DrawPrintBitmap(Bitmap bmp, PrintPageEventArgs e, PrintJobImageOption drawOptions)
        {
            /* http://stackoverflow.com/questions/8761633/how-to-find-the-actual-printable-area-printdocument */

            var isLandscape = drawOptions.LandScape;
            if (drawOptions.DetectOrientation)
            {
                var isXbiggerPaper = PageSettings.PaperSize.Width >= PageSettings.PaperSize.Height;
                var isXbiggerImage = bmp.Width >= bmp.Height;
                isLandscape = isXbiggerImage == isXbiggerPaper;
            }

            var bmpPrint = bmp;
            if (!isLandscape)
            {
                bmpPrint = bmpPrint.RotateToNewSize(e.PageSettings.PrinterSettings.LandscapeAngle);
            }


            var ps = e.PageSettings;
            var g = e.Graphics;
            var dpiX = drawOptions.UsePageResolution ? ps.PrinterResolution.X : bmpPrint.HorizontalResolution;
            var dpiY = drawOptions.UsePageResolution ? ps.PrinterResolution.Y : bmpPrint.VerticalResolution;
            
            var printableArea = e.PageSettings.PrintableArea;
            if (_printAction == PrintAction.PrintToPreview)
                g.TranslateTransform(printableArea.X, printableArea.Y);

            var units = GraphicsUnit.Display;
            var bmpBounds = bmpPrint.GetBounds(ref units);

            RectangleF newImageSizeFit;
            if (drawOptions.Scaled)
            {
                var cX = PageSettings.PaperSize.Width -
                             (drawOptions.IgnoreMargins ? 0 : PageSettings.Margins.Left + PageSettings.Margins.Right);
                var cY = PageSettings.PaperSize.Height -
                         (drawOptions.IgnoreMargins ? 0 : PageSettings.Margins.Top + PageSettings.Margins.Bottom);
                var oX = bmpBounds.Width;
                var oY = bmpBounds.Height;

                newImageSizeFit = !drawOptions.CropMaximizedScale ? 
                    new RectangleF(new PointF(0,0), MathUtil.GetNewSizeToFitInsideContainer(cX, cY, oX, oY)) : 
                    MathUtil.GetNewSizeToFitContainerWithMinimalLoss(cX, cY, oX, oY);
            }
            else
            {
                newImageSizeFit = new RectangleF(0, 0, bmpBounds.Width, bmpBounds.Height);
            }

            double tX, tY;
            switch (drawOptions.HorizontalAlignment)
            {
                case PrintJobImageHorizontalAlignment.Left:
                    tX = 0;
                    break;
                case PrintJobImageHorizontalAlignment.Center:
                    tX = (PageSettings.PaperSize.Width - newImageSizeFit.Width) / 2;
                    break;
                case PrintJobImageHorizontalAlignment.Right:
                    tX = PageSettings.PaperSize.Width - newImageSizeFit.Width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (drawOptions.VerticalAlignment)
            {
                case PrintJobImageVerticalAlignment.Top:
                    tY = 0;
                    break;
                case PrintJobImageVerticalAlignment.Center:
                    tY = (PageSettings.PaperSize.Height - newImageSizeFit.Height) / 2;
                    break;
                case PrintJobImageVerticalAlignment.Bottom:
                    tY = PageSettings.PaperSize.Height - newImageSizeFit.Height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Ajustement pour les marges
            tX -= e.PageSettings.Margins.Left;
            tY -= e.PageSettings.Margins.Top;
            
            bmpPrint.SetResolution(dpiX, dpiY);
            var dest = new RectangleF((float) tX, (float) tY, newImageSizeFit.Width, newImageSizeFit.Height);

            try
            {
                g.DrawImage(bmpPrint, dest, bmpBounds, units);
            }
            catch (Exception err)
            {
                log.Error(err, "Could not draw with following parameters: bmp=({0}x{1}, dpi={2}), dest={3}, source={4}",
                    bmpPrint.Width, bmpPrint.Height, bmpPrint.HorizontalResolution.ToString(CultureInfo.InvariantCulture),
                    dest, bmpBounds);
            }
            
            if (DebugPrintAreaForInstanceBitmaps)
            {
                g.DrawRectangle(Pens.Black,
                    printableArea.X - e.PageSettings.Margins.Left,
                    printableArea.Y - e.PageSettings.Margins.Top,
                    (!ps.Landscape ? printableArea.Width : printableArea.Height) - 1,
                    (!ps.Landscape ? printableArea.Height : printableArea.Width) - 1);
            }
        }

        /// <summary>
        /// Ajoute une liste de chemins de fichiers comme images à imprimer. Une image par page.
        /// Les images sont ouvertes lors de leur impression.
        /// Provoque une exception si le fichier n'existe pas.
        /// Lance une exception si on est en train d'imprimer.
        /// </summary>
        /// <param name="bitmapFiles">La collection de chemin d'images à utiliser.</param>
        /// <param name="drawOptions">Options d'impression pour l'ensembles des images données.</param>
        /// <param name="fileResolution">Résolution du fichier.</param>
        public void SetActionsDrawBitmaps(IEnumerable<string> bitmapFiles, PrintJobImageOption drawOptions = null, float? fileResolution = null)
        {
            if (IsPrinting) throw new InvalidOperationException("Cannot modify print actions during impression.");

            _pageDrawActions.Clear();
            foreach (var file in bitmapFiles)
            {
                AddActionDrawBitmap(file, drawOptions, fileResolution);
            }
        }

        /// <summary>
        /// Ajoute un chemin d'image à la fin de la liste d'impression.
        /// Provoque une exception si le fichier n'existe pas.
        /// Lance une exception si on est en train d'imprimer.
        /// </summary>
        /// <param name="bmpFile">Le chemin de l'image à imprimer.</param>
        /// <param name="drawOptions">Options d'impression.</param>
        /// <param name="fileResolution">Résolution du fichier (DPI) à la lecture. 300x300 par défaut.</param>
        public void AddActionDrawBitmap(string bmpFile, PrintJobImageOption drawOptions = null, float? fileResolution = null)
        {
            if (IsPrinting) throw new InvalidOperationException("Cannot modify print actions during impression.");

            if (!File.Exists(bmpFile))
            {
                throw new FileNotFoundException("Bitmap file does not exists: " + bmpFile);
            }

            if (drawOptions == null)
            {
                drawOptions = DefaultImageOption ?? new PrintJobImageOption();
            }

            _pageDrawActions.Add(e =>
            {
                var res = fileResolution ?? DEFAULT_RESOLUTION;

                // Vérification pour des cas rares où le fichier est utilisé. Par exemple, on tente d'imprimer en même temps que l'on tente
                // D'envoyer un fichier vers le serveur.
                var maxWait = DateTime.Now.AddMilliseconds(1000);
                bool fail;
                do
                {
                    Stream stream = null;
                    try
                    {
                        stream = new FileInfo(bmpFile).Open(FileMode.Open, FileAccess.Read, FileShare.None);
                        fail = false;
                    }
                    catch (IOException)
                    {
                        fail = true;
                    }
                    finally
                    {
                        stream?.Close();
                    }
                } while (DateTime.Now.ToBinary() < maxWait.ToBinary() && fail);
                
                var bmp = FileUtil.LoadBitmap(bmpFile, res);

                DrawPrintBitmap(bmp, e, drawOptions);
            });
        }

        /// <summary>
        /// Vide la liste d'actions, pour réutiliser la même configuration d'impression.
        /// </summary>
        public void CleanPrintActions()
        {
            if (IsPrinting) throw new InvalidOperationException("Cannot modify print actions during impression.");
            _pageDrawActions.Clear();
        }

        /// <summary>
        /// Indique que les éléments nécessaires à l'impression sont prêts et valides, et que l'on n'est présentement pas
        /// en train d'imprimer. On peut lancer l'impression.
        /// </summary>
        public bool IsReadyToPrint => !(PrinterSettings == null || !PrinterSettings.IsValid ||
                                      PageSettings == null ||
                                      PrintDocument == null ||
                                      !_pageDrawActions.Any() || IsPrinting);

        /// <summary>
        /// Indique si on est présentement en train d'imprimer.
        /// </summary>
        public bool IsPrinting { get; private set; }

        /// <summary>
        /// Nombre total de pages à imprimer.
        /// </summary>
        public int TotalPages => _pageDrawActions.Count;

        /// <summary>
        /// La configuration de l'imprimante.
        /// </summary>
        public PrinterSettings PrinterSettings { get; }

        /// <summary>
        /// Configuration de page
        /// </summary>
        public PageSettings PageSettings { get; }

        /// <summary>
        /// Configuration de document d'impression.
        /// </summary>
        public PrintDocument PrintDocument { get; }

        /// <summary>
        /// Lance l'exécution de la tâche d'impression.
        /// Si les paramètres d'impressions sont incorrects ou si l'on imprime déjà, cette méthode va lancer une exception.
        /// </summary>
        /// <param name="ct">Token optionnel pour stopper l'impression en cours de route. n'est utilisé que pour l'invocation actuelle.</param>
        /// <returns>TRUE si l'impression s'est bien déroulée et que le fichier a été imprimé. FALSE si stoppé manuellement ou si
        /// une erreur s'est produite.</returns>
        public bool DoWork(CancellationToken? ct)
        {
            if (!IsReadyToPrint)
            {
                throw new Exception("Printer, Page and Document settings are invalid.");
            }

            IsPrinting = true;
            _ct = ct;
            var stopped = false;
            _currentPage = 0;
            _lastPrintStatus = false;

            try
            {
                PrintDocument.Print();
            }
            catch (Exception e)
            {
                log.Error(e);
                stopped = true;
            }

            if (_ct != null)
            {
                stopped |= _ct.Value.IsCancellationRequested;
            }

            _ct = null;
            IsPrinting = false;
            _lastPrintStatus = !stopped;
            return _lastPrintStatus;
        }

        private void PrintPageDo(object sender, PrintPageEventArgs e)
        {
            if (_ct != null && _ct.Value.IsCancellationRequested)
            {
                e.Cancel = true;
                return;
            }
            if (_currentPage >= _pageDrawActions.Count) return;

            var currAction = _pageDrawActions[_currentPage];
            currAction.Invoke(e);
            _currentPage++;
            e.HasMorePages = _currentPage < _pageDrawActions.Count;

            PagePrinted?.Invoke(this, _currentPage);
        }

        private void EndPrintDo(object sender, PrintEventArgs e)
        {
            PrintFinished?.Invoke(this, _lastPrintStatus);
        }

        private void BeginPrintDo(object sender, PrintEventArgs e)
        {
            _printAction = e.PrintAction;
            PrintStarted?.Invoke(this, null);
        }

        /// <summary>
        /// Créer une référence d'option d'impression complète à partir des configurations actuelles.
        /// </summary>
        /// <param name="specificOption">On peut passer des options d'impression spécifiques.</param>
        /// <returns>Une instance d'option d'impression complète.</returns>
        public PrintJobInfo CreatePrintJobInfo(PrintJobImageOption specificOption = null)
        {
            return new PrintJobInfo
            {
                PaperSize = PageSettings.PaperSize,
                PrinterName = PrinterSettings.PrinterName,
                DrawOptions = specificOption ?? DefaultImageOption
            };
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as PrintJob;
            return Equals(other);
        }

        /// <inheritdoc />
        public bool Equals(PrintJob other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(DefaultImageOption, other.DefaultImageOption) && 
                PrintUtil.Equals(PrinterSettings, other.PrinterSettings) &&
                PrintUtil.Equals(PageSettings, other.PageSettings);
        }
        
        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DefaultImageOption?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (PrinterSettings?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (PageSettings?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Surcharge d'opérateur
        /// </summary>
        /// <param name="left">Opérande gauche</param>
        /// <param name="right">Opérane droite</param>
        /// <returns>Égalité</returns>
        public static bool operator ==(PrintJob left, PrintJob right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Surcharge d'opérateur
        /// </summary>
        /// <param name="left">Opérande gauche</param>
        /// <param name="right">Opérane droite</param>
        /// <returns>Non-Égalité</returns>
        public static bool operator !=(PrintJob left, PrintJob right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }
}