using System;
using NLog;
using NomadLogic.iBrand.Common.Util.LaserBeam;

namespace iBrand_SnapShot.Trigger
{
    internal class LaserBeamTriggerAbstract : TriggerAbstract
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private LaserBeamManager _manager;

        public override void Start()
        {
            if (_manager != null && !_manager.IsRunning)
            {
                _manager.Start();
            }
        }

        public override void Stop()
        {
            if (_manager != null && _manager.IsRunning)
            {
                _manager.Stop();
            }
        }

        public override void Dispose()
        {
            try
            {
                if (_manager != null)
                {
                    _manager.Stop();
                    _manager.BeamOccluded -= FireTriggerEvent;
                    _manager.Dispose();
                }
            }
            catch (Exception e)
            {
                log.Debug(e);
            }
        }

        public override bool Init(string[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                throw new Exception("Requires at least two parameters.");
            }

            var btnNumber = int.Parse(parameters[1]);

            try
            {
                if (_manager == null)
                {
                    _manager = new LaserBeamManager();
                    _manager.BeamOccluded += FireTriggerEvent;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (_manager != null)
            {
                _manager.GamepadName = parameters[0];
                _manager.TriggerButton = btnNumber;
            }
            return _manager?.Init() ?? false;
        }

        public override bool IsInitialized()
        {
            return _manager != null && _manager.IsInitialized;
        }

        public override bool IsRunning()
        {
            return _manager != null && _manager.IsRunning;
        }
    }
}
