using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class StartProjectDialog : Form
    {

        public event EventHandler OnOkClick;

        public StartProjectDialog(Project project)
        {
            InitializeComponent();
            Init();
            cancelButton.Click += OnCancel;
            okButton.Click += OnOk;

            questionnaireComboBox.SelectedIndexChanged += OnQuestionnaireSelectionChanged;
            eventComboBox.SelectedIndexChanged += OnEventSelectionChanged;

            PopulateQuestionnairesComboBox(project);
        }

        private void Init()
        {
            cancelButton.Text = Resources.iBrand.Cancel;
            okButton.Text = Resources.iBrand.Ok;
            questionnaire_label.Text = Resources.iBrand.Questionnaire;
            userNameTextBox.KeyPress += userNameTextBox_KeyPress;
        }

        void userNameTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                OnOk(sender, e);
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            StartPosition = FormStartPosition.Manual;
            var x = WorkingArea.Container.Location.X + WorkingArea.Container.Width / 2 - Size.Width / 2;
            var y = WorkingArea.Container.Location.Y + (WorkingArea.Container.Height - Height) / 2;
            Location = new Point(x, y);
        }

        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnOk(object sender, EventArgs e)
        {
            if (ValidateEntry())
            {
                OnOkClick?.Invoke(this, EventArgs.Empty);

                //this.DialogResult = DialogResult.OK;
                //this.Close();
            }
        }

        private void OnQuestionnaireSelectionChanged(object sender, EventArgs e)
        {
            var q = (QuestionnaireInfo)questionnaireComboBox.SelectedItem;
            PopulateEventsComboBox(q);
        }

        private void OnEventSelectionChanged(object sender, EventArgs e)
        {
            userNameTextBox.Focus();
        }

        public QuestionnaireInfo SelectedQuestionnaireInfo => (QuestionnaireInfo)questionnaireComboBox.SelectedItem;

        public IbrandEventInfo SelectedIbrandEventInfo => (IbrandEventInfo)eventComboBox.SelectedItem;

        public string UserName => userNameTextBox.Text;

        private void PopulateEventsComboBox(QuestionnaireInfo q)
        {
            var events = q.Events;

            if (events == null) return;
            eventComboBox.Items.Clear();

            foreach (var ev in events)
            {
                eventComboBox.Items.Add(ev);
            }

            if (events.Count != 1) return;
            eventComboBox.SelectedIndex = 0;
            eventComboBox.Enabled = false;
        }

        private void PopulateQuestionnairesComboBox(Project project)
        {
            var questionnaires = project.QuestionnaireInfos;

            if (questionnaires == null) return;
            foreach (var q in questionnaires)
            {
                questionnaireComboBox.Items.Add(q);
            }

            if (questionnaires.Count != 1) return;
            questionnaireComboBox.SelectedIndex = 0;
            questionnaireComboBox.Enabled = false;
        }

        private bool ValidateEntry()
        {
            if (questionnaireComboBox.SelectedIndex == -1)
            {
                MsgBox.Show(this, Resources.iBrand.Questionnaire, Resources.iBrand.AppName);
                return false;
            }

            if (eventComboBox.SelectedIndex == -1)
            {
                MsgBox.Show(this, Resources.iBrand.SelectQuestionnaireError, Resources.iBrand.AppName);
                return false;
            }

            if (userNameTextBox.Text.Length == 0)
            {
                MsgBox.Show(this, Resources.iBrand.EnterUserName, Resources.iBrand.AppName);
                return false;
            }

            return true;
        }
    }
}