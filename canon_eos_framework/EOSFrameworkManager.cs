using System;
using System.Linq;
using System.Threading;

namespace Canon.Eos.Framework
{
    /// <summary>
    /// https://github.com/esskar/Canon.Eos.Framework/tree/master/Canon.Eos.Framework
    /// </summary>
    public sealed class EosFrameworkManager
    {
        private EosFramework _framework;

        public event EventHandler CameraAdded;

        public EosCamera[] GetCameras()
        {
            using (var cameras = _framework.GetCameraCollection())
                return cameras.ToArray();
        }

        public void LoadFramework()
        {
            if (_framework != null) return;

            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.MTA)
                throw new Exception("Thread apartment state for EosFramework is not MTA. Framework will not load.");

            _framework = new EosFramework();
            _framework.CameraAdded += HandleCameraAdded;
        }

        public void ReleaseFramework()
        {
            if (_framework == null) return;
            _framework.CameraAdded -= HandleCameraAdded;
            _framework.Dispose();
        }

        public EosCamera GetLastConnectedCamera()
        {
            return _framework.GetLastConnectedCamera();
        }

        private void HandleCameraAdded(object sender, EventArgs eventArgs)
        {
            CameraAdded?.Invoke(this, eventArgs);
        }
    }
}
