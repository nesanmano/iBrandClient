using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Win32;
using NomadLogic.PrintStation.Contest;

namespace NomadLogic.PrintStation.Ui
{
    public partial class PopUpWinner : Form
    {
        public PopUpWinner(int userId, [NotNull] ContestParameters contestParams)
        {
            InitializeComponent();

            var backgroundName = UiFilesUtil.GetFileName(UiFilesUtil.PopUpWinner, userId);
            Size = new Size(1000, 500);
            picture_box.Location = new Point(0, 0);
            picture_box.Size = Size;

            if (File.Exists(backgroundName))
            {
                picture_box.ImageLocation = backgroundName;
            }

            picture_box.BringToFront();

            if (string.IsNullOrWhiteSpace(contestParams.WinnerDialogPassword))
            {
                picture_box.MouseDown += (sender, args) => { Close(); };
                return;
            }

            // ReSharper disable once UnusedVariable
            var onlyNumeric = int.TryParse(contestParams.WinnerDialogPassword, out int test);

            // Uniquement si on a un mot de passe valide.
            var keyboard = onlyNumeric ? 
                new NumbersKeyboardControl(-1, 240, 200, 240) : 
                new KeyboardControl(-1, 240, 960, 240);

            keyboard.Parent = picture_box;
            keyboard.BringToFront();
            keyboard.Show();

            var textbox = new TextBox
            {
                Size = new Size(200, 40),
                Location = new Point((1000 - 200) / 2, 200),
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            textbox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode != Keys.Enter) return;
                if (contestParams.WinnerDialogPassword.Equals(textbox.Text))
                {
                    FormUtil.SafeCall(this, Close);
                }
            };
            textbox.TextChanged += (sender, args) =>
            {
                if (contestParams.WinnerDialogPassword.Equals(textbox.Text))
                {
                    FormUtil.SafeCall(this, Close);
                }
            };

            Controls.Add(textbox);
            textbox.BringToFront();

            Load += (sender, args) => ActiveControl = textbox;

        }
    }
}