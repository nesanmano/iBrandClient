using System;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util.Trigger;
using Phidget22;
using Phidget22.Events;

namespace NomadLogic.Common.Phidgets
{
    [PublicAPI]
    public class PhidgetTriggerManager: TriggerManagerBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        private DigitalInput _digiInput;
        private bool _sendEvents;

        public int TriggerChannel { get; set; }

        /// <summary>
        /// Configure un numéro de série spécifique pour cette instance, au cas où de multiples Phidget du même type existent
        /// sur une seule machine.
        /// </summary>
        public int? InputSerial { get; set; }

        public int? HubPort { get; set; }
        
        public event Action TriggerReady;
        
        public event EventHandler<string> TriggerError;

        public override bool IsInitialized { get; protected set; }

        public override bool Init()
        {
            if (_digiInput != null) return true;

            _digiInput = new DigitalInput();

            _digiInput.Attach += Input_attach;
            _digiInput.Detach += Input_detach;
            _digiInput.Error += Input_error;
            _digiInput.StateChange += Input_change;

            try
            {
                //set all the values grabbed from command line.  these values have defaults that are set in ExampleUtils.cs, you can check there to see them.
                _digiInput.Channel = TriggerChannel; //selects the channel on the device to open
                if (InputSerial.HasValue) _digiInput.DeviceSerialNumber = InputSerial.Value; //selects the device or hub to open
                if (HubPort.HasValue) _digiInput.HubPort = HubPort.Value; //selects the port on the hub to open

                _digiInput.IsLocal = true;

                _digiInput.Open(); //open the device specified by the above parameters
            }
            catch (PhidgetException ex)
            {
                try
                {
                    _digiInput?.Close();
                }
                catch (Exception)
                {
                    // die nice
                }

                log.Error(ex, "Could not initialize Phidget controller.");
                return false;
            }

            IsInitialized = true;
            return true;
        }

        public override void Start()
        {
            _sendEvents = _digiInput != null;
        }

        public override void Stop()
        {
            _sendEvents = false;
        }

        private void Input_change(object sender, DigitalInputStateChangeEventArgs e)
        {
            if (!_sendEvents) return;
            if (!(sender is DigitalInput die)) return;
            if (!die.State) return;

            log.Trace("Phidget '{0}' triggered.", die.DeviceSerialNumber);
            InvokeTrigger();
        }
        
        private void Input_error(object sender, ErrorEventArgs e)
        {
            if (!(sender is DigitalInput die)) return;
            log.Trace("Phidget '{0}' warned of error: {1}-'{2}'", die.DeviceSerialNumber, e.Code, e.Description);
            TriggerError?.Invoke(this, e.Description);
        }

        private void Input_detach(object sender, DetachEventArgs e)
        {
            if (!(sender is DigitalInput die)) return;
            log.Trace("Phidget '{0}' is now detached.", die.DeviceSerialNumber);
        }

        private void Input_attach(object sender, AttachEventArgs e)
        {
            if (!(sender is DigitalInput die)) return;
            log.Trace("Phidget '{0}' is now attached.", die.DeviceSerialNumber);
            if (log.IsDebugEnabled) TriggerReport(new TriggerManagerReport{StatusMessage = "Phidget trigger ready: " + die.DeviceSerialNumber}, true);
            TriggerReady?.Invoke();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_digiInput != null)
            {
                _digiInput.Attach -= Input_attach;
                _digiInput.Detach -= Input_detach;
                _digiInput.Error -= Input_error;
                _digiInput.StateChange -= Input_change;

                _digiInput.Close();
                _digiInput = null;
            }

            IsInitialized = false;
        }

        public override bool IsRunning
        {
            get => _sendEvents;
            protected set => _sendEvents = value;
        }

        public override string DefaultConfigFileName => "phidget_trigger_manager.json";
        
        /// <summary>
        /// Une fois la connexion faite, donne le numéro de série de l'appareil connecté.
        /// </summary>
        public int GetDigitalInputSerial()
        {
            return _digiInput?.DeviceSerialNumber ?? -1;
        }

        /// <summary>
        /// Obtient la différence temporelle avant le prochain événement possible.
        /// Une valeur négative veut dire que le système est prêt depuis un certain temp déjà.
        /// </summary>
        public TimeSpan GetNextPossibleTriggerTime()
        {
            var now = DateTime.Now;
            var next = GetNextTimeForTrigger() ?? now;
            return next - now;
        }
    }
}