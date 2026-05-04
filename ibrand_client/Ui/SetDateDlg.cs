using System;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class SetDateDlg : Form
    {
        public event EventHandler OnCancelClick;
        public event EventHandler OnOkClick;

        public SetDateDlg()
        {
            InitializeComponent();
            Init();
            PopulateTimeZoneComboBox();
            okButton.Click += OnOk;
            cancelButton.Click += OnCancel;
        }

        private void Init()
        {
            date_label.Text = Resources.iBrand.Date;
            time_label.Text = Resources.iBrand.Time;
            time_zone_label.Text = Resources.iBrand.TimeZone;
        }

        private void PopulateTimeZoneComboBox()
        {
            /*TimeZoneCollection timeZones = new TimeZoneCollection();
            timeZones.Initialize();
            int selected_index = -1;
            TimeZoneInformation tzi = new TimeZoneInformation();
            DateTimeHelper.GetTimeZoneInformation(ref tzi);
            for (int i = 0; i < timeZones.Count; i++)
            {
                TimeZoneInformation timeZoneInfo = (TimeZoneInformation)timeZones[i];
                if (timeZoneInfo != null)
                {
                    try
                    {
                        if (isValidTimeZone(timeZoneInfo))
                        {
                            this.timeZonePicker.Items.Add(timeZoneInfo);
                            if (timeZoneInfo.StandardName != null &&
                                timeZoneInfo.StandardName.CompareTo(tzi.StandardName) == 0)
                                selected_index = timeZonePicker.Items.Count - 1;
                        }
                    }
                    catch (System.Exception e)
                    {
                        if (timeZoneInfo.DisplayName != null)
                            this.timeZonePicker.Items.Add(timeZoneInfo);
                    }
                }
            }
            timeZonePicker.SelectedIndex = selected_index;*/
        }

        public DateTime Date => datePicker.Value;

        public DateTime Time => timePicker.Value;

        public TimeZoneInformation TimeZone => (TimeZoneInformation)timeZonePicker.SelectedItem;

        private bool IsValidTimeZone(TimeZoneInformation tzi)
        {
            string[] permittedStarts = { "GMT-10", "GMT-9", "GMT-8", "GMT-7", "GMT-6", "GMT-5", "GMT-4", "GMT-3", "GMT-3:30" };
            try
            {
                var valide = false;
                foreach (var start in permittedStarts)
                {
                    if (tzi.DisplayName.StartsWith(start))
                        valide = true;
                }
                return valide;
            }
            catch (System.Exception)
            {
                return true;
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            if (OnCancelClick != null)
                OnCancelClick(this, EventArgs.Empty);
        }

        private void OnOk(object sender, EventArgs e)
        {
            if (OnOkClick != null)
                OnOkClick(this, EventArgs.Empty);
        }
    }
}