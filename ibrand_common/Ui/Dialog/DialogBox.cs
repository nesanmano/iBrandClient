//=============================================================================
// COPYRIGHT: Prosoft-Lanz
//=============================================================================
//
// $Workfile: DialogBox.cs $
//
// PROJECT : CodeProject Components
// VERSION : 1.00
// CREATION : 19.02.2003
// AUTHOR : JCL
//
// DETAILS : DialogBoxes centered into the parent owner.
//           This class implement the following objects:
//
//   DlgBox.ShowDialog(...)		for CommonDialog and Form
//   MsgBox.Show(...)			for standard MessageBox
//   AppBox.Show(...)			for standard MessageBox with ProductName as caption
//	 ErrBox.Show(...)			for standard error MessageBox
//
//-----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NLog;

namespace NomadLogic.Common.Ui.Dialog
{
	///////////////////////////////////////////////////////////////////////
	#region DlgBox

	/// <summary>
	/// Class to display a CommonDialog or modal Form centered on the owner.
	/// </summary>
	/// <example>
	/// This example display the default print dialog box in the center of the parent.
	/// <code>
	/// PrintDialog printDlg = new PrintDialog();
	/// if (DlgBox.ShowDialog(printDlg, parent) == DialogResult.OK)
	///   printDocument.Print();
	/// </code>
	/// </example>
	public static class DlgBox
	{
	    // To remove the constructor from the documentation!

		///////////////////////////////////////////////////////////////////////
		// CommonDialog

		/// <summary>
		/// Show a command dialog box at the center of the active window.
		/// </summary>
		public static DialogResult ShowDialog(CommonDialog dlg)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var dlgResult = dlg.ShowDialog();
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// Show a command dialog box at the center of the owner window.
		/// </summary>
		public static DialogResult ShowDialog(CommonDialog dlg, IWin32Window owner)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var dlgResult = dlg.ShowDialog();
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// Form

		/// <summary>
		/// Show a form dialog box at the center of the active window.
		/// </summary>
		public static DialogResult ShowDialog(Form form)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var dlgResult = form.ShowDialog();
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// Show a form dialog box at the center of the owner window.
		/// </summary>
		public static DialogResult ShowDialog(Form form, IWin32Window owner)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var dlgResult = form.ShowDialog();
			centerWindow.Dispose();
			return dlgResult;
		}

        public static DialogResult ShowDialogCenterTop(Form form, IWin32Window owner)
        {
            var handle = owner?.Handle ?? IntPtr.Zero;
            var centerWindow = new CenterTopWindow(handle);
            var dlgResult = form.ShowDialog();
            centerWindow.Dispose();
            return dlgResult;
        }

        public static DialogResult ShowDialogCenterTop(Form form)
        {
            var centerWindow = new CenterTopWindow(IntPtr.Zero);
            var dlgResult = form.ShowDialog();
            centerWindow.Dispose();
            return dlgResult;
        }
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region MsgBox

	/// <summary>
	/// Class to display a MessageBox centered on the owner.
	/// </summary>
	/// <remarks>
	/// Same methods as the standard MessageBox.
	/// </remarks>
	/// <example>
	/// This example display a "Hello" message box centered on the owner.
	/// <code>
	/// MsgBox.Show("Hello");
	/// </code>
	/// </example>
	public static class MsgBox
	{
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
	    // To remove the constructor from the documentation!

		///////////////////////////////////////////////////////////////////////
		// text

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(string text)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(text, caption);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(owner, text, caption);
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, caption

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(string text, string caption)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var dlgResult = MessageBox.Show(text, caption);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text, string caption)
		{
            IntPtr handle;
            try
            {
                handle = owner?.Handle ?? IntPtr.Zero;
            } catch (Exception) {
                handle = IntPtr.Zero;
            }
			var centerWindow = new CenterWindow(handle);
			var dlgResult = MessageBox.Show(owner, text, caption);
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, caption, buttons

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var dlgResult = MessageBox.Show(text, caption, buttons);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var dlgResult = MessageBox.Show(owner, text, caption, buttons);
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, caption, buttons, defaultButton

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var dlgResult = MessageBox.Show(text, caption, buttons, icon);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(Control owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
            if (owner == null || owner.IsDisposed) return DialogResult.Abort;

            var handle = IntPtr.Zero;
            try
            {
                handle = owner.Handle;
            } catch (Exception e)
            {
                log.Error(e);
            }

		        var centerWindow = new CenterWindow(handle);
		        var dlgResult = MessageBox.Show(owner, text, caption, buttons, icon);
		        centerWindow.Dispose();
		        return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, caption, buttons, defaultButton, icon

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var dlgResult = MessageBox.Show(text, caption, buttons, icon, defaultButton);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var dlgResult = MessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, caption, buttons, defaultButton, icon, options

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var dlgResult = MessageBox.Show(text, caption, buttons, icon, defaultButton, options);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var dlgResult = MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
			centerWindow.Dispose();
			return dlgResult;
		}
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region AppBox

	/// <summary>
	/// Class to display a MessageBox centered on the owner.
	/// The MessageBox caption is always Application.ProductName.
	/// </summary>
	/// <remarks>
	/// Same methods as the standard MessageBox without caption.
	/// </remarks>
	/// <example>
	/// This example display an application message box centered on the owner.
	/// <code>
	/// AppBox.Show("Hello");
	/// </code>
	/// </example>
	public static class AppBox
	{
	    // To remove the constructor from the documentation!

		///////////////////////////////////////////////////////////////////////
		// text

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(string text)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(text, caption);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(owner, text, caption);
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, buttons

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(string text, MessageBoxButtons buttons)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(text, caption, buttons);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text, MessageBoxButtons buttons)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(owner, text, caption, buttons);
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, buttons, defaultButton

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(string text, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(text, caption, buttons, icon);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(owner, text, caption, buttons, icon);
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, buttons, defaultButton, icon

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(string text, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(text, caption, buttons, icon, defaultButton);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
			centerWindow.Dispose();
			return dlgResult;
		}

		///////////////////////////////////////////////////////////////////////
		// text, buttons, defaultButton, icon, options

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(string text, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			var centerWindow = new CenterWindow(IntPtr.Zero);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(text, caption, buttons, icon, defaultButton, options);
			centerWindow.Dispose();
			return dlgResult;
		}

		/// <summary>
		/// See MSDN MessageBox() method. Caption is Application.ProductName.
		/// </summary>
		public static DialogResult Show(IWin32Window owner, string text, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			var handle = owner?.Handle ?? IntPtr.Zero;
			var centerWindow = new CenterWindow(handle);
			var caption = Application.ProductName;
			var dlgResult = MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
			centerWindow.Dispose();
			return dlgResult;
		}
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region ErrBox

	/// <summary>
	/// Class to display application error MessageBox centered on the owner.
	/// The caption of the MessageBox is Application.ProductName.
	/// </summary>
	/// <example>
	/// This example display an error message box centered on the owner.
	/// <code>
	/// ErrBox.Show(ex);
	/// </code>
	/// </example>
	public static class ErrBox
	{
	    // To remove the constructor from the documentation!

		/// <summary>
		/// Show an error MessageBox with an icon error and an OK button.
		/// </summary>
		/// <param name="err">The error message.</param>
		/// <param name="owner">The owner of the error MessageBox.</param>
		/// <returns>Dialog result of the MessageBox.</returns>
		public static DialogResult Show(Control owner, string err)
		{
			var caption = Application.ProductName;
			return MsgBox.Show(owner, err, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>
		/// Show an error MessageBox with an icon error and an OK button.
		/// </summary>
		/// <param name="err">The error message.</param>
		/// <returns>Dialog result of the MessageBox.</returns>
		public static DialogResult Show(string err)
		{
			var caption = Application.ProductName;
			return MsgBox.Show(err, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>
		/// Show an error MessageBox with exception message, an icon error and an OK button.
		/// </summary>
		/// <param name="ex">Exception to be displayed.</param>
		/// <returns>Dialog result of the MessageBox.</returns>
		public static DialogResult Show(Exception ex)
		{
			var err = ex.Message;
			while (ex.InnerException != null)
			{
				ex = ex.InnerException;
				err += Environment.NewLine;
				err += ex.Message;
			}
			var caption = Application.ProductName;
			return MsgBox.Show(err, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		/// <summary>
		/// Show a specialized error MessageBox centered into the parent owner.
		/// </summary>
		/// <param name="ex">Exception to be displayed.</param>
		/// <param name="debugMode">true to display the full informations else false.</param>
		/// <returns>Dialog result of the MessageBox.</returns>
        public static DialogResult Show(Exception ex, bool debugMode)
		{
		    if (debugMode)
				return Show(ex);
		    return Show(ex.Message);
		}
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region CenterWindow class

	internal sealed class CenterWindow
	{
		public IntPtr _hOwner;
		private Rectangle _rect;

		public CbtHook _cbtHook;
		public WndProcRetHook _wndProcRetHook;

		public CenterWindow(IntPtr hOwner)
		{
			_hOwner = hOwner;
			_cbtHook = new CbtHook();
			_cbtHook.WindowActivate += WndActivate;
			_cbtHook.Install();
		}

		public void Dispose()
		{
			if (_wndProcRetHook != null)
			{
				_wndProcRetHook.Uninstall();
				_wndProcRetHook = null;
			}
			if (_cbtHook != null)
			{
				_cbtHook.Uninstall();
				_cbtHook = null;
			}
		}

		public void WndActivate(object sender, CbtEventArgs e)
		{
			var hMsgBox = e._w_param;

			// try to find a howner for this message box
			if (_hOwner == IntPtr.Zero)
				_hOwner = User32.GetActiveWindow();

			// Get the MessageBox window rect
			var rectDlg = new Rect();
			User32.GetWindowRect(hMsgBox, ref rectDlg);

			// Get the owner window rect
			var rectForm = new Rect();
			User32.GetWindowRect(_hOwner, ref rectForm);

			// Get the biggest screen area
			var rectScreen = Api.TrueScreenRect;

			// if no parent window, center on the primary screen
			if (rectForm._right == rectForm._left)
				rectForm._right = rectForm._left = Screen.PrimaryScreen.WorkingArea.Width / 2;
			if (rectForm._bottom == rectForm._top)
				rectForm._bottom = rectForm._top = Screen.PrimaryScreen.WorkingArea.Height / 2;

			// center on parent
			var dx = ((rectDlg._left + rectDlg._right) - (rectForm._left + rectForm._right)) / 2;
			var dy = ((rectDlg._top + rectDlg._bottom) - (rectForm._top + rectForm._bottom)) / 2;

			_rect = new Rectangle(
				rectDlg._left - dx,
				rectDlg._top - dy,
				rectDlg._right - rectDlg._left,
				rectDlg._bottom - rectDlg._top);

			// place in the screen
			if (_rect.Right > rectScreen.Right) _rect.Offset(rectScreen.Right - _rect.Right, 0);
			if (_rect.Bottom > rectScreen.Bottom) _rect.Offset(0, rectScreen.Bottom - _rect.Bottom);
			if (_rect.Left < rectScreen.Left) _rect.Offset(rectScreen.Left - _rect.Left, 0);
			if (_rect.Top < rectScreen.Top) _rect.Offset(0, rectScreen.Top - _rect.Top);

			if (e._is_dialog)
			{
				// do the job when the WM_INITDIALOG message returns
				_wndProcRetHook = new WndProcRetHook(hMsgBox);
				_wndProcRetHook.WndProcRet += WndProcRet;
				_wndProcRetHook.Install();
			}
			else
				User32.MoveWindow(hMsgBox, _rect.Left, _rect.Top, _rect.Width, _rect.Height, 1);

			// uninstall this hook
			var wndHook = (WindowsHook)sender;
			Debug.Assert(_cbtHook == wndHook);
			_cbtHook.Uninstall();
			_cbtHook = null;
		}

		public void WndProcRet(object sender, WndProcRetEventArgs e)
		{
			if (e._cw.message == WndMessage.WmInitdialog ||
				e._cw.message == WndMessage.WmUnknowinit)
			{
				User32.MoveWindow(e._cw.hwnd, _rect.Left, _rect.Top, _rect.Width, _rect.Height, 1);
				
				// uninstall this hook
				var wndHook = (WindowsHook)sender;
				Debug.Assert(_wndProcRetHook == wndHook);
				_wndProcRetHook.Uninstall();
				_wndProcRetHook = null;
			}
		}
	}
	#endregion

    #region CenterTopWindow class

    internal sealed class CenterTopWindow
    {
        public IntPtr _hOwner;
        private Rectangle _rect;

        public CbtHook _cbtHook;
        public WndProcRetHook _wndProcRetHook;

        public CenterTopWindow(IntPtr hOwner)
        {
            _hOwner = hOwner;
            _cbtHook = new CbtHook();
            _cbtHook.WindowActivate += WndActivate;
            _cbtHook.Install();
        }

        public void Dispose()
        {
            if (_wndProcRetHook != null)
            {
                _wndProcRetHook.Uninstall();
                _wndProcRetHook = null;
            }
            if (_cbtHook != null)
            {
                _cbtHook.Uninstall();
                _cbtHook = null;
            }
        }

        public void WndActivate(object sender, CbtEventArgs e)
        {
            var hMsgBox = e._w_param;

            // try to find a howner for this message box
            if (_hOwner == IntPtr.Zero)
                _hOwner = User32.GetActiveWindow();

            // Get the MessageBox window rect
            var rectDlg = new Rect();
            User32.GetWindowRect(hMsgBox, ref rectDlg);

            // Get the owner window rect
            var rectForm = new Rect();
            User32.GetWindowRect(_hOwner, ref rectForm);

            // Get the biggest screen area
            var rectScreen = Api.TrueScreenRect;

            // if no parent window, center on the primary screen
            if (rectForm._right == rectForm._left)
                rectForm._right = rectForm._left = Screen.PrimaryScreen.WorkingArea.Width / 2;
            if (rectForm._bottom == rectForm._top)
                rectForm._bottom = rectForm._top = Screen.PrimaryScreen.WorkingArea.Top;

            // center on parent
            var dx = ((rectDlg._left + rectDlg._right) - (rectForm._left + rectForm._right)) / 2;
            var dy = rectForm._top;//((rectDlg.top + rectDlg.bottom) - (rectForm.top + rectForm.bottom)) / 2;

            _rect = new Rectangle(
                rectDlg._left - dx,
                rectDlg._top - dy,
                rectDlg._right - rectDlg._left,
                rectDlg._bottom - rectDlg._top);

            // place in the screen
            if (_rect.Right > rectScreen.Right) _rect.Offset(rectScreen.Right - _rect.Right, 0);
            if (_rect.Bottom > rectScreen.Bottom) _rect.Offset(0, rectScreen.Bottom - _rect.Bottom);
            if (_rect.Left < rectScreen.Left) _rect.Offset(rectScreen.Left - _rect.Left, 0);
            if (_rect.Top < rectScreen.Top) _rect.Offset(0, rectScreen.Top - _rect.Top);

            if (e._is_dialog)
            {
                // do the job when the WM_INITDIALOG message returns
                _wndProcRetHook = new WndProcRetHook(hMsgBox);
                _wndProcRetHook.WndProcRet += WndProcRet;
                _wndProcRetHook.Install();
            }
            else
                User32.MoveWindow(hMsgBox, _rect.Left, _rect.Top, _rect.Width, _rect.Height, 1);

            // uninstall this hook
            var wndHook = (WindowsHook)sender;
            Debug.Assert(_cbtHook == wndHook);
            _cbtHook.Uninstall();
            _cbtHook = null;
        }

        public void WndProcRet(object sender, WndProcRetEventArgs e)
        {
            if (e._cw.message == WndMessage.WmInitdialog ||
                e._cw.message == WndMessage.WmUnknowinit)
            {
                User32.MoveWindow(e._cw.hwnd, _rect.Left, _rect.Top, _rect.Width, _rect.Height, 1);

                // uninstall this hook
                var wndHook = (WindowsHook)sender;
                Debug.Assert(_wndProcRetHook == wndHook);
                _wndProcRetHook.Uninstall();
                _wndProcRetHook = null;
            }
        }
    }
    #endregion
}
