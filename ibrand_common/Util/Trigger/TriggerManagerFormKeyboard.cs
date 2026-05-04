using System.Collections.Generic;
using System.Linq;
using NLog;
using NomadLogic.Common.Util.Keyboard;

namespace NomadLogic.Common.Util.Trigger
{
    public class TriggerManagerFormKeyboard : TriggerManagerBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        private GlobalKeyboardHook _globalKeyboardHook;

        public List<KeyboardHook> Hooks { get; set; } = new List<KeyboardHook>
        {
            new KeyboardHook
            {
                Key = VirtualKeys.Escape
                /* TODO Capture des touches système simultanées pas encore intégré.
                ,AdditionalFlags = new List<VirtualKeys>
                {
                    VirtualKeys.Control,
                    VirtualKeys.Shift
                }
                */
            }
        };

        private void ControlOnKeyPress(object sender, GlobalKeyboardHookEventArgs e)
        {
            if (e == null) return;

            log.Trace("Keyboard trigger detected: " + e.Hook);
            InvokeTrigger();
        }

        public override void Dispose()
        {
            if (_globalKeyboardHook == null) return;
            _globalKeyboardHook.KeyboardHookPressed -= ControlOnKeyPress;
            _globalKeyboardHook.Dispose();
        }

        public override bool IsRunning { get; protected set; }

        public override bool IsInitialized { get; protected set; }

        public override bool Init()
        {
            if (_globalKeyboardHook != null) return true; // Déjà init
            if (Hooks == null || !Hooks.Any())
            {
                log.Error("No keyboard hook defined");
                return false;
            }

            _globalKeyboardHook = new GlobalKeyboardHook(Hooks);
            _globalKeyboardHook.KeyboardHookPressed += ControlOnKeyPress;
            IsInitialized = true;
            return true;
        }

        public override void Start()
        {
            IsRunning = true;
        }

        public override void Stop()
        {
            IsRunning = false;
        }

        public override string DefaultConfigFileName { get; } = "trigger_keyboard_config.json";
    }
}