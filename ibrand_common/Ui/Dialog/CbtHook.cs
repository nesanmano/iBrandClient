//=============================================================================
// COPYRIGHT: Prosoft-Lanz
//=============================================================================
//
// $Workfile: CbtHook.cs $
//
// PROJECT : CodeProject Components
// VERSION : 1.00
// CREATION : 19.02.2003
// AUTHOR : JCL
//
// DETAILS : This class implement the WH_CBT Windows hook mechanism.
//           From MSDN, Dino Esposito.
//           WindowCreate, WindowDestroy and WindowActivate user events.
//
//-----------------------------------------------------------------------------

using System;
using System.Text;

namespace NomadLogic.Common.Ui.Dialog
{
	///////////////////////////////////////////////////////////////////////
	#region Enum CbtHookAction

	/// <summary>
	/// CBT hook actions.
	/// </summary>
	internal enum CbtHookAction
	{
		HcbtMovesize = 0,
		HcbtMinmax = 1,
		HcbtQs = 2,
		HcbtCreatewnd = 3,
		HcbtDestroywnd = 4,
		HcbtActivate = 5,
		HcbtClickskipped = 6,
		HcbtKeyskipped = 7,
		HcbtSyscommand = 8,
		HcbtSetfocus = 9
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class CbtEventArgs

	/// <summary>
	/// Class used for WH_CBT hook event arguments.
	/// </summary>
	public class CbtEventArgs : EventArgs
	{
		/// wParam parameter.
		public IntPtr _w_param;
		/// lParam parameter.
		public IntPtr _l_param;
		/// Window class name.
		public string _class_name;
		/// True if it is a dialog window.
		public bool _is_dialog;

		internal CbtEventArgs(IntPtr wParam, IntPtr lParam)
		{
			// cache the parameters
			_w_param = wParam;
			_l_param = lParam;

			// cache the window's class name
			StringBuilder sb = new StringBuilder();
			sb.Capacity = 256;
			User32.GetClassName(wParam, sb, 256);
			_class_name = sb.ToString();
			_is_dialog = (_class_name == "#32770");
		}
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class CbtHook
	
	/// <summary>
	/// Class to expose the windows WH_CBT hook mechanism.
	/// </summary>
	public class CbtHook : WindowsHook
	{
		/// <summary>
		/// WH_CBT hook delegate method.
		/// </summary>
		public delegate void CbtEventHandler(object sender, CbtEventArgs e);

		/// <summary>
		/// WH_CBT create event.
		/// </summary>
		public event CbtEventHandler WindowCreate;
		/// <summary>
		/// WH_CBT destroy event.
		/// </summary>
		public event CbtEventHandler WindowDestroye;
		/// <summary>
		/// WH_CBT activate event.
		/// </summary>
		public event CbtEventHandler WindowActivate;

		/// <summary>
		/// Construct a WH_CBT hook.
		/// </summary>
		public CbtHook() : base(HookType.WhCbt)
		{
			HookInvoke += CbtHookInvoked;
		}
		/// <summary>
		/// Construct a WH_CBT hook giving a hook filter delegate method.
		/// </summary>
		/// <param name="func">Hook filter event.</param>
		public CbtHook(HookProc func) : base(HookType.WhCbt, func)
		{
			HookInvoke += CbtHookInvoked;
		}

		// handles the hook event
		private void CbtHookInvoked(object sender, HookEventArgs e)
		{
			// handle hook events (only a few of available actions)
			switch ((CbtHookAction)e._code)
			{
				case CbtHookAction.HcbtCreatewnd:
					HandleCreateWndEvent(e._w_param, e._l_param);
					break;
				case CbtHookAction.HcbtDestroywnd:
					HandleDestroyWndEvent(e._w_param, e._l_param);
					break;
				case CbtHookAction.HcbtActivate:
					HandleActivateEvent(e._w_param, e._l_param);
					break;
			}
		}

		// handle the CREATEWND hook event
		private void HandleCreateWndEvent(IntPtr wParam, IntPtr lParam)
		{
			if (WindowCreate != null)
			{
				CbtEventArgs e = new CbtEventArgs(wParam, lParam);
				WindowCreate(this, e);
			}
		}

		// handle the DESTROYWND hook event
		private void HandleDestroyWndEvent(IntPtr wParam, IntPtr lParam)
		{
			if (WindowDestroye != null)
			{
				CbtEventArgs e = new CbtEventArgs(wParam, lParam);
				WindowDestroye(this, e);
			}
		}

		// handle the ACTIVATE hook event
		private void HandleActivateEvent(IntPtr wParam, IntPtr lParam)
		{
			if (WindowActivate != null)
			{
				CbtEventArgs e = new CbtEventArgs(wParam, lParam);
				WindowActivate(this, e);
			}
		}
	}
	#endregion
}
