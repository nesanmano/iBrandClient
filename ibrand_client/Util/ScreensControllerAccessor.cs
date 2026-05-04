using NomadLogic.iBrandDesktop.Ui;

namespace NomadLogic.iBrandDesktop.Util
{
    public static class ScreensControllerAccessor
    {
        private static TouchScreenController _screensController;

        private static TouchScreenControllerWhitoutProject _screensControllerWhitoutProject;

        public static TouchScreenController ScreensController
        {
            get
            {
                return _screensController;
            }

            set
            {
                _screensController = value;
            }
        }

        public static TouchScreenControllerWhitoutProject ScreensControllerWithoutProject
        {
            get
            {
                return _screensControllerWhitoutProject;
            }

            set
            {
                _screensControllerWhitoutProject = value;
            }
        }
    }
}
