//=============================================================================
// COPYRIGHT: Prosoft-Lanz
//=============================================================================
//
// $Workfile: WndProcRetHook.cs $
//
// PROJECT : CodeProject Components
// VERSION : 1.00
// CREATION : 19.02.2003
// AUTHOR : JCL
//
// DETAILS : This class implement the WH_CALLWNDPROCRET Windows hook mechanism.
//           From MSDN, Dino Esposito.
//
//           WindowCreate, WindowDestroye and WindowActivate user events.
//
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace NomadLogic.Common.Ui.Dialog
{
	///////////////////////////////////////////////////////////////////////
	#region Enum WndMessage

	/// <summary>
	/// windows message.
	/// </summary>
	public enum WndMessage
	{
		/// Sent to the dialog procedure immediately before the dialog is displayed.
		WmInitdialog = 0x0110,
		/// Sent to the dialog procedure immediately before the dialog is displayed.
		WmUnknowinit = 0x0127
	}
	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class WndProcRetEventArgs

	/// Class used for WH_CALLWNDPROCRET hook event arguments.
	public class WndProcRetEventArgs : EventArgs
	{
		/// wParam parameter.
		public IntPtr _w_param;
		/// lParam parameter.
		public IntPtr _l_param;
		/// CWPRETSTRUCT structure.
		public CwPRetStruct _cw;

		internal WndProcRetEventArgs(IntPtr wParam, IntPtr lParam)
		{
			_w_param = wParam;
			_l_param = lParam;
			_cw = new CwPRetStruct();
			Marshal.PtrToStructure(lParam, _cw);
		}
	}

	/// <summary>
	/// CWPRETSTRUCT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public class CwPRetStruct
	{
		/// Return value.
		public int lResult;
		/// lParam parameter.
		public int lParam;
		/// wParam parameter.
		public int wParam;
		/// Specifies the message.
		public WndMessage message;
		/// Handle to the window that processed the message.
		public IntPtr hwnd;
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class WndProcRetHook
	
	/// <summary>
	/// Class to expose the windows WH_CALLWNDPROCRET hook mechanism.
	/// </summary>
	public class WndProcRetHook : WindowsHook
	{
		/// <summary>
		/// WH_CALLWNDPROCRET hook delegate method.
		/// </summary>
		public delegate void WndProcEventHandler(object sender, WndProcRetEventArgs e);

		private readonly IntPtr _hWndHooked;

		/// <summary>
		/// Window procedure event.
		/// </summary>
		public event WndProcEventHandler WndProcRet;

		/// <summary>
		/// Construct a WH_CALLWNDPROCRET hook.
		/// </summary>
		/// <param name="hWndHooked">
		/// Handle of the window to be hooked. IntPtr.Zero to hook all window.
		/// </param>
		public WndProcRetHook(IntPtr hWndHooked) : base(HookType.WhCallwndprocret)
		{
			_hWndHooked = hWndHooked;
			HookInvoke += WndProcRetHookInvoked;
		}
		/// <summary>
		/// Construct a WH_CALLWNDPROCRET hook giving a hook filter delegate method.
		/// </summary>
		/// <param name="hWndHooked">
		/// Handle of the window to be hooked. IntPtr.Zero to hook all window.
		/// </param>
		/// <param name="func">Hook filter event.</param>
		public WndProcRetHook(IntPtr hWndHooked, HookProc func) : base(HookType.WhCallwndprocret, func)
		{
			_hWndHooked = hWndHooked;
			HookInvoke += WndProcRetHookInvoked;
		}

		// handles the hook event
		private void WndProcRetHookInvoked(object sender, HookEventArgs e)
		{
			var wpe = new WndProcRetEventArgs(e._w_param, e._l_param);
			if ((_hWndHooked == IntPtr.Zero || wpe._cw.hwnd == _hWndHooked))
				WndProcRet?.Invoke(this, wpe);
		}
	}
	#endregion
}
