//=============================================================================
// COPYRIGHT: Prosoft-Lanz
//=============================================================================
//
// $Workfile: WindowsHook.cs $
//
// PROJECT : CodeProject Components
// VERSION : 1.00
// CREATION : 19.02.2003
// AUTHOR : JCL
//
// DETAILS : This class implement the Windows hook mechanism.
//           From MSDN, Dino Esposito.
//
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NomadLogic.Common.Ui.Dialog
{
	///////////////////////////////////////////////////////////////////////
	#region Class HookEventArgs

	/// Class used for hook event arguments.
	public class HookEventArgs : EventArgs
	{
		/// Event code parameter.
		public int _code;
		/// wParam parameter.
		public IntPtr _w_param;
		/// lParam parameter.
		public IntPtr _l_param;

		internal HookEventArgs(int code, IntPtr wParam, IntPtr lParam)
		{
			_code = code;
			_w_param = wParam;
			_l_param = lParam;
		}
	}
	
	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Enum HookType

	/// Hook Types.
	public enum HookType
	{
		/// <value>0</value>
		WhJournalrecord = 0,
		/// <value>1</value>
		WhJournalplayback = 1,
		/// <value>2</value>
		WhKeyboard = 2,
		/// <value>3</value>
		WhGetmessage = 3,
		/// <value>4</value>
		WhCallwndproc = 4,
		/// <value>5</value>
		WhCbt = 5,
		/// <value>6</value>
		WhSysmsgfilter = 6,
		/// <value>7</value>
		WhMouse = 7,
		/// <value>8</value>
		WhHardware = 8,
		/// <value>9</value>
		WhDebug = 9,
		/// <value>10</value>
		WhShell = 10,
		/// <value>11</value>
		WhForegroundidle = 11,
		/// <value>12</value>
		WhCallwndprocret = 12,		
		/// <value>13</value>
		WhKeyboardLl = 13,
		/// <value>14</value>
		WhMouseLl = 14
	}
	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class WindowsHook

	/// <summary>
	/// Class to expose the windows hook mechanism.
	/// </summary>
	public class WindowsHook
	{
		/// <summary>
		/// Hook delegate method.
		/// </summary>
		public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

		// internal properties
		internal IntPtr _h_hook = IntPtr.Zero;
		internal HookProc _filter_func;
		internal HookType _hook_type;

		/// <summary>
		/// Hook delegate method.
		/// </summary>
		public delegate void HookEventHandler(object sender, HookEventArgs e);

		/// <summary>
		/// Hook invoke event.
		/// </summary>
		public event HookEventHandler HookInvoke;

		internal void OnHookInvoke(HookEventArgs e)
		{
			if (HookInvoke != null)
				HookInvoke(this, e);
		}

		/// <summary>
		/// Construct a HookType hook.
		/// </summary>
		/// <param name="hook">Hook type.</param>
		public WindowsHook(HookType hook)
		{
			_hook_type = hook;
			_filter_func = CoreHookProc;
		}
		/// <summary>
		/// Construct a HookType hook giving a hook filter delegate method.
		/// </summary>
		/// <param name="hook">Hook type</param>
		/// <param name="func">Hook filter event.</param>
		public WindowsHook(HookType hook, HookProc func)
		{
			_hook_type = hook;
			_filter_func = func; 
		}

		// default hook filter function
		internal int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
		{
			if (code < 0)
				return CallNextHookEx(_h_hook, code, wParam, lParam);

			// let clients determine what to do
			HookEventArgs e = new HookEventArgs(code, wParam, lParam);
			OnHookInvoke(e);

			// yield to the next hook in the chain
			return CallNextHookEx(_h_hook, code, wParam, lParam);
		}

		/// <summary>
		/// Install the hook. 
		/// </summary>
		public void Install()
		{
			_h_hook = SetWindowsHookEx(_hook_type, _filter_func, IntPtr.Zero,  Thread.CurrentThread.ManagedThreadId);//(int)AppDomain.GetCurrentThreadId());
		}

		
		/// <summary>
		/// Uninstall the hook.
		/// </summary>
 		public void Uninstall()
		{
			if (_h_hook != IntPtr.Zero)
			{
				UnhookWindowsHookEx(_h_hook);
				_h_hook = IntPtr.Zero;
			}
		}

		#region Win32 Imports

		[DllImport("user32.dll")]
		internal static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadId);

		[DllImport("user32.dll")]
		internal static extern int UnhookWindowsHookEx(IntPtr hhook); 

		[DllImport("user32.dll")]
		internal static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);

		#endregion
	}
	#endregion
}
