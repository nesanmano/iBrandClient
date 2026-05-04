using System;
using NomadLogic.iBrandDesktop.Ui;

//using OpenNETCF.WindowsCE;

namespace NomadLogic.iBrandDesktop
{
    public class SetDateController
    {
        SetDateDlg _setDateDlg;

        public void OnSetDate(object sender, EventArgs e)
        {
            _setDateDlg = new SetDateDlg();
            _setDateDlg.OnCancelClick += OnCancel;
            _setDateDlg.OnOkClick += OnOk;

            _setDateDlg.ShowDialog();
        }

        private void OnOk(object sender, EventArgs e)
        {
            var date = _setDateDlg.Date;
            var time = _setDateDlg.Time;
            var newDateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

            /*TimeZoneInformation tzi = (TimeZoneInformation)_set_date_dlg.TimeZone;
            DateTimeHelper.SetTimeZoneInformation(tzi);
            DateTimeHelper.LocalTime = new_date_time;*/
            
            _setDateDlg.Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            _setDateDlg.Close();
        }
    }
}
