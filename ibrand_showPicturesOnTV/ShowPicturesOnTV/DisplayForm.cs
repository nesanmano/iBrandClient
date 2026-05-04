using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Configuration;
using NomadLogic.Common.Services.LocalCapabilities;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.Win32;
using NomadLogic.ShowPicturesOnTV.DisplayControllers;

namespace NomadLogic.ShowPicturesOnTV
{
    public sealed partial class DisplayForm : Form, ILocalCapabilityTester
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IDisplayController _dc;
        private readonly CancellationTokenSource _cts;
        private int _nbrMouseUp;

        public IList<ILocalCapabilityVerifier> CapabilityVerifiers { get; set; }
        
        public void VerifyCapabilities()
        {
            if (CapabilityVerifiers == null || !CapabilityVerifiers.Any()) return;
            
            var config = ServicesContainer.GetService<NomadConfiguration>();

            // Assigne les vérificateurs de forme avec la référence de cet object
            foreach (var cv in CapabilityVerifiers)
            {
                if (!(cv is ILocalCapabilityVerifierForm cvf)) continue;
                cvf.ParentForm = this;
            }
            
            LocalCapabilitiesManager.AnalyseCapabilities(CapabilityVerifiers, config, capResult =>
            {
                if (capResult.Groups.All(g => g.Status == StatusEnum.Valid)) return;
                var sb = new StringBuilder();
                sb.AppendLine(@"Some capabilities were detected as invalid or missing, some critical functions may not work as intended. Do you wish to stop the execution of the application?");
                foreach (var gr in capResult.Groups)
                {
                    foreach (var message in gr.ErrorMessages)
                    {
                        sb.AppendLine(message);
                    }
                }

                Invoke(new Action(() =>
                {
                    var diagResult = MessageBox.Show(this, sb.ToString(), @"Missing capabilities", MessageBoxButtons.YesNo);
                    if (diagResult == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                }));
            });
        }
        
        public DisplayForm()
        {
            InitializeComponent();

            var configFile = DirectoryUtil.GetLocalApplicationDir() + ConfigurationShowPictureOnTv.ConfigFileName;
            ConfigurationShowPictureOnTv configuration;

            try
            {
                if (File.Exists(configFile))
                {
                    configuration = JsonUtil.ReadJsonFile<ConfigurationShowPictureOnTv>(new FileInfo(configFile));
                }
                else
                {
                    configuration = new ConfigurationShowPictureOnTv();
                    JsonUtil.WriteJsonFile(new FileInfo(configFile), configuration, new JsonSerializerSettings 
                    {
                        NullValueHandling = NullValueHandling.Include // Force l'apparition de tous les champs.
                    });
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                configuration = new ConfigurationShowPictureOnTv();
            }

            var readMeFile = DirectoryUtil.GetLocalApplicationDir() + "ReadMeConfig.txt";
            if (!File.Exists(readMeFile))
            {
                File.Copy(DirectoryUtil.GetCurrentApplicationInstallDir() + "ReadMeConfig.txt", readMeFile);
            }

            Bounds = Screen.AllScreens.Length > configuration.ScreenIdToDisplay ? Screen.AllScreens[configuration.ScreenIdToDisplay].Bounds : Screen.AllScreens[0].Bounds;
            StartPosition = FormStartPosition.Manual;
            BackgroundImageLayout = ImageLayout.Zoom;
            
            var panel = new Panel
            {
                Parent = this,
                Size = new Size(Bounds.Width, Bounds.Height)
            };

            // Initialisation de l'image de fond
            if (!string.IsNullOrWhiteSpace(configuration.BackgroundFile))
            {
                Bitmap imageLoaded = null;
                // Tente de localiser le fichier.
                // 1- Est-ce que le fichier est en forme absolue?
                if (File.Exists(configuration.BackgroundFile))
                {
                    try
                    {
                        imageLoaded = FileUtil.LoadBitmap(configuration.BackgroundFile);
                    }
                    catch (Exception e)
                    {
                        log.Error(e, "Background image file does not load as a Bitmap: " + configuration.BackgroundFile);
                        imageLoaded = null;
                    }
                }
                else
                {
                    // 2- Dans le répertoire de l'application, car relatif?
                    var tmpFilePath = DirectoryUtil.GetLocalApplicationDir() + configuration.BackgroundFile;

                    if (File.Exists(tmpFilePath))
                    {
                        try
                        {
                            imageLoaded = FileUtil.LoadBitmap(tmpFilePath);
                        }
                        catch (Exception e)
                        {
                            log.Error(e, "Background image file does not load as a Bitmap: " + configuration.BackgroundFile);
                            imageLoaded = null;
                        }
                    }
                }

                if (imageLoaded == null)
                {
                    // Une erreur est survenue quelque part, on se contente d'un fond noir.
                    panel.BackColor = Color.Black;
                }
                else
                {
                    // On charge l'image et son mode d'affichage.
                    panel.BackgroundImageLayout = configuration.BackgroundFileLayout;
                    panel.BackgroundImage = imageLoaded;
                }
            }
            else
            {
                panel.BackColor = Color.Black;
            }

            KeyDown += OnKeyDown;
            
            _cts = new CancellationTokenSource();
            switch(configuration.ControllerType)
            {
                case ConfigurationShowPictureOnTvControllerType.PictureBundle:
                    _dc = new ImageBundleController();
                    break;
                case ConfigurationShowPictureOnTvControllerType.SingleImage:
                    _dc = new ImageController();
                    break;
                case ConfigurationShowPictureOnTvControllerType.Video:
                    _dc = new VideoController();
                    break;
                default:
                    log.Fatal("Could not instanciate display controller: Unknown Enumeration used: " + configuration.ControllerType);
                    Application.Exit(); // On ferme l'application.
                    break;
            }

            if (_dc == null) return;

            var mainFolder = configuration.PictureDir;

            if (string.IsNullOrWhiteSpace(mainFolder)) // pas de configuration, on utilise le défaut de l'application.
            {
                mainFolder = ConfigurationShowPictureOnTv.PictureDirDefault;
            }

            if (!Directory.Exists(mainFolder))
            {
                mainFolder = DirectoryUtil.GetLocalApplicationDir() + mainFolder; // On tente un path relatif.

                if (!Directory.Exists(mainFolder))
                {
                    throw new Exception("Main directory not found: " + mainFolder); // Aucun autre essai; on détruit.
                }
            }

            log.Debug("Initialize display controller on following directory: {0}", mainFolder);

            var sb = new StringBuilder();
            sb.Append(mainFolder);
            if (mainFolder.Last() != Path.DirectorySeparatorChar) sb.Append(Path.DirectorySeparatorChar);
            sb.Append("shown");
            var shownFolder = sb.ToString();

            if (!Directory.Exists(shownFolder))
                Directory.CreateDirectory(shownFolder);

            if (!Directory.Exists(shownFolder))
            {
                throw new Exception("Could not create shown output directory: " + shownFolder);
            }

            _dc.DisplayConfiguration = configuration;
            _dc.OnCompleteStop += OnDisplayControllerStop;
            _dc.DisplayPanel = panel;
            _dc.MainFolder = mainFolder;
            _dc.ShownFolder = shownFolder;
            _dc.OnMouseEvent += OnMouseUp;
            _dc.Init();

            Load += (sender, args) =>
            {
                VerifyCapabilities();
                log.Trace("Capabilities verified.");

                //FormUtil.SetWindowPos(this, WinUserConstants.HWND_CONSTANTS.HWND_NOTOPMOST, 0, 0, FormUtil.ScreenX, FormUtil.ScreenY, WinUserConstants.SWP_PARAMS_UFLAGS.SWP_SHOWWINDOW);
                FormUtil.SetWindowPos(this, WinUserConstants.HWND_CONSTANTS.HWND_NOTOPMOST, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height, WinUserConstants.SWP_PARAMS_UFLAGS.SWP_SHOWWINDOW);
                
                new Task(() => _dc.LookForDisplayItems(_cts.Token)).Start();
            };
        }

        private static void OnDisplayControllerStop()
        {
            Application.Exit();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Escape) return;
            e.SuppressKeyPress = true;
            log.Trace("Escape key pressed.");
            _cts.Cancel();
            _dc.Stop();
        }
        
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            log.Trace("Mouse Up pressed.");
            _nbrMouseUp++;

            if (_nbrMouseUp < 4) return;
            _cts.Cancel();
            _dc.Stop();
        }
    }
}
