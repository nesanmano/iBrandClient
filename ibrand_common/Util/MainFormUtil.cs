using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.Common.Util
{
    public class MainFormUtil
    {
        private static MainFormUtil _formUtil;

        private FormWindowState _winState;
        private FormBorderStyle _brdStyle;
        private bool _topMost;
        private Rectangle _bounds;


        private MainFormUtil()
        {
        }

        public static MainFormUtil GetInstance()
        {
            return _formUtil ?? (_formUtil = new MainFormUtil());
        }

        public Form MainForm { set; get; }

        public bool IsMaximized { get; private set; }

        public void Maximize()
        {
            if (MainForm == null || MainForm.Handle == IntPtr.Zero) return;
            if (IsMaximized) return;
            IsMaximized = true;
            Save();
            MainForm.WindowState = FormWindowState.Maximized;
            MainForm.FormBorderStyle = FormBorderStyle.None;
            //_main_form.TopMost = true;
            // FormUtil.SetWinFullScreen(MainForm.Handle);
            FormUtil.SetWindowPos(
                MainForm, 
                WinUserConstants.HWND_CONSTANTS.HWND_TOP, 
                0,0, FormUtil.ScreenX, FormUtil.ScreenY,
                WinUserConstants.SWP_PARAMS_UFLAGS.SWP_SHOWWINDOW);
        }

        private void Save()
        {
            _winState = MainForm.WindowState;
            _brdStyle = MainForm.FormBorderStyle;
            _topMost = MainForm.TopMost;
            _bounds = MainForm.Bounds;
        }

        public void Restore()
        {
            if (MainForm == null) return;
            if (!IsMaximized) return;
            MainForm.WindowState = _winState;
            MainForm.FormBorderStyle = _brdStyle;
            MainForm.TopMost = _topMost;
            MainForm.Bounds = _bounds;
            IsMaximized = false;
        }
    }
}
