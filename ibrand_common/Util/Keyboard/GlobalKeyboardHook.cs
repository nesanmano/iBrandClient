using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace NomadLogic.Common.Util.Keyboard
{
    public class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        public GlobalKeyboardHookEventArgs(
            KeyboardHook keyboardData,
            GlobalKeyboardHook.KeyboardState keyboardState)
        {
            Hook = keyboardData;
            KeyboardState = keyboardState;
        }

        public GlobalKeyboardHook.KeyboardState KeyboardState { get; }
        public KeyboardHook Hook { get; }
    }

    //Based on https://gist.github.com/Stasonix
    // Voir aussi https://stackoverflow.com/questions/1198161/where-to-get-the-keycode-for-keyboard-hook-in-c-sharp
    [PublicAPI]
    public class GlobalKeyboardHook : IDisposable
    {
        //const int HC_ACTION = 0;
        [PublicAPI]
        public enum KeyboardState
        {
            KeyDown = 0x0100,
            KeyUp = 0x0101,
            SysKeyDown = 0x0104,
            SysKeyUp = 0x0105
        }

        public const int WH_KEYBOARD_LL = 13;
        private const int KfAltdown = 0x2000;
        public const int LlkhfAltdown = KfAltdown >> 8;

        // EDT: Replaced VkSnapshot(int) with RegisteredKeys(Keys[])
        private readonly List<KeyboardHook> _registeredKeys;
        private HookProc _hookProc;
        private IntPtr _user32LibraryHandle;

        private IntPtr _windowsHookHandle;
        
        // EDT: Added an optional parameter (registeredKeys) that accepts keys to restict
        // the logging mechanism.
        /// <summary>
        /// </summary>
        /// <param name="registeredKeys">Keys that should trigger logging. Pass null for full logging.</param>
        public GlobalKeyboardHook([NotNull]List<KeyboardHook> registeredKeys)
        {
            _registeredKeys = registeredKeys;
            _windowsHookHandle = IntPtr.Zero;
            _user32LibraryHandle = IntPtr.Zero;
            _hookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

            _user32LibraryHandle = LoadLibrary("User32");
            int errorCode;
            if (_user32LibraryHandle == IntPtr.Zero)
            {
                errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }
            
            _windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, _user32LibraryHandle, 0);
            if (_windowsHookHandle != IntPtr.Zero) return;
            
            errorCode = Marshal.GetLastWin32Error();
            throw new Win32Exception(errorCode,
                $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
           
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardHookPressed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                // because we can unhook only in the same thread, not in garbage collector thread
                if (_windowsHookHandle != IntPtr.Zero)
                {
                    if (!UnhookWindowsHookEx(_windowsHookHandle))
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode,
                            $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }

                    _windowsHookHandle = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    _hookProc -= LowLevelKeyboardProc;
                }

            if (_user32LibraryHandle == IntPtr.Zero) return;

            if (!FreeLibrary(_user32LibraryHandle)) // reduces reference to library by 1.
            {
                var errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            _user32LibraryHandle = IntPtr.Zero;
            
        }

        ~GlobalKeyboardHook()
        {
            Dispose(false);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        ///     The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain.
        ///     You would install a hook procedure to monitor the system for certain types of events. These events are
        ///     associated either with a specific thread or with all threads in the same desktop as the calling thread.
        /// </summary>
        /// <param name="idHook">hook type</param>
        /// <param name="lpfn">hook procedure</param>
        /// <param name="hMod">handle to application instance</param>
        /// <param name="dwThreadId">thread identifier</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure.</returns>
        [DllImport("USER32", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        /// <summary>
        ///     The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx
        ///     function.
        /// </summary>
        /// <param name="hHook">handle to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [DllImport("USER32", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);
        
        /// <summary>
        ///     The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
        ///     A hook procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="hHook">handle to current hook</param>
        /// <param name="code">hook code passed to hook procedure</param>
        /// <param name="wParam">value passed to hook procedure</param>
        /// <param name="lParam">value passed to hook procedure</param>
        /// <returns>If the function succeeds, the return value is true.</returns>
        [DllImport("USER32", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var fEatKeyStroke = false;

            var wparamTyped = wParam.ToInt32();
            // On ne regarde que KeyPressed, KeyReleased, SysKeyPress, SysKeyRelease
            if (!Enum.IsDefined(typeof(KeyboardState), wparamTyped)) return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);

            var o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
            var p = (LowLevelKeyboardInputEvent) o;
                
            KeyboardHook kh = null;
            foreach (var regKey in _registeredKeys)
            {
                var keyVal = (int) regKey.Key;
                var boardVal = (int) p.Key;
                if (boardVal == keyVal)
                {
                    // TODO Présentement, la logique ne capture pas les touches systèmes toujours pressées.
                    // À faire plus tard. Je crois qu'il faut les capturer individuellement
                    if (regKey.AdditionalFlags.All(f => (int)f == (p.Flags & (int)f)))
                    {
                        kh = regKey;
                        break;
                    }
                }
            }

            if (kh != null)
            {
                var handler = KeyboardHookPressed;
                var eventArguments = new GlobalKeyboardHookEventArgs(kh, (KeyboardState) wparamTyped);
                handler?.Invoke(this, eventArguments);

                fEatKeyStroke = eventArguments.Handled;
            }

            return fEatKeyStroke ? (IntPtr) 1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct LowLevelKeyboardInputEvent
        {
            /// <summary>
            ///     A virtual-key code. The code must be a value in the range 1 to 254.
            /// </summary>
            public int VirtualCode;

            // EDT: added a conversion from VirtualCode to Keys.
            /// <summary>
            ///     The VirtualCode converted to typeof(Keys) for higher usability.
            /// </summary>
            public Keys Key => (Keys) VirtualCode;

            /// <summary>
            ///     A hardware scan code for the key.
            /// </summary>
            public int HardwareScanCode;

            /// <summary>
            ///     The extended-key flag, event-injected Flags, context code, and transition-state flag. This member is specified as
            ///     follows. An application can use the following values to test the keystroke Flags. Testing LLKHF_INJECTED (bit 4)
            ///     will tell you whether the event was injected. If it was, then testing LLKHF_LOWER_IL_INJECTED (bit 1) will tell you
            ///     whether or not the event was injected from a process running at lower integrity level.
            /// </summary>
            public int Flags;

            /// <summary>
            ///     The time stamp stamp for this message, equivalent to what GetMessageTime would return for this message.
            /// </summary>
            public int TimeStamp;

            /// <summary>
            ///     Additional information associated with the message.
            /// </summary>
            public IntPtr AdditionalInformation;
        }
    }
}