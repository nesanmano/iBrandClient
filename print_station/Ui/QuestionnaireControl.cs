using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.PrintStation.Ui
{
    public partial class QuestionnaireControl : UserControl
    {
        public QuestionnaireControl(int width, int height)
        {
            InitializeComponent();

            Size = new Size(width, height);

            InitScreenPanelSize();
        }

        public Panel ScreenPanel { get; private set; }

        private void InitScreenPanelSize()
        {
            ScreenPanel.Size = new Size(Width, Height);
        }
    }
}