using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Ui.Localization;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Print;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.Common.Ui
{
    /// <summary>
    /// Dialogue où l'usager choisit entre quatre images différentes pour tester l'impression.
    /// </summary>
    public partial class PrintTestDialog : LocalizedForm
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [PublicAPI]
        public string ImagesLocation { get; set; } = DirectoryUtil.GetLocalNomadLogicAssetsDir();

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            pict4x6.ImageLocation = ImagesLocation + @"printTest_4x6_t.jpg";
            pict4x6x2.ImageLocation = ImagesLocation + @"printTest_(4x6)x2_t.jpg";
            pict5x7.ImageLocation = ImagesLocation + @"printTest_5x7_t.jpg";
            pict8_5x11.ImageLocation = ImagesLocation + @"printTest_8_5x11_t.jpg";

            pict4x6.Click += OnPictureClick;
            pict4x6x2.Click += OnPictureClick;
            pict5x7.Click += OnPictureClick;
            pict8_5x11.Click += OnPictureClick;
        }

        private void OnPictureClick(object sender, EventArgs e)
        {
            string fileToprint = null;

            if (sender == pict4x6)
            {
                fileToprint = "printTest_4x6.jpg";
            }
            else if (sender == pict4x6x2)
            {
                fileToprint = "printTest_(4x6)x2.jpg";
            }
            else if (sender == pict5x7)
            {
                fileToprint = "printTest_5x7.jpg";
            }
            else if (sender == pict8_5x11)
            {
                fileToprint = "printTest_8_5x11.jpg";
            }

            var file = ImagesLocation + fileToprint;

            this.SafeCall(() => LaunchPrint(file));
        }

        private void LaunchPrint(string file)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                MessageBox.Show(this, @"Could not find print test file : " + (file ?? ""), @"PrintStation", MessageBoxButtons.OK);
            }
            else
            {
                var success = false;
                try
                {
                    success = PrintUtil.Print(file, null, this);
                }
                catch (Exception err)
                {
                    log.Error(err, "Error during impression test.");
                }
                finally
                {
                    if (!success)
                    {
                        MessageBox.Show(this, @"Could not print test image.", @"PrintStation", MessageBoxButtons.OK);
                    }
                }
            }

            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        [STAThread]
        private void BtnCustomFile_Click(object sender, EventArgs e)
        {
            string file = null;

            var t = new Thread(() =>
            {
                var diag = new OpenFileDialog()
                {
                    Multiselect = false,
                    AddExtension = false,
                    CheckFileExists = true,
                    Filter = @"Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|Portable File Documents (*.pdf)|*.pdf"
                };

                if (diag.ShowDialog() != DialogResult.OK) return;
                file = diag.FileName;
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            if (!string.IsNullOrWhiteSpace(file)) this.SafeCall(() => LaunchPrint(file));
        }

        /// <inheritdoc />
        protected override void InitializeControlTags()
        {
            btnCancel.Tag = "UI_Cancel";
        }

        /// <inheritdoc />
        protected override string GetLocalizedTextFromManager(string tag)
        {
            return "Todo";
        }
    }
}
