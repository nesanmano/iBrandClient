using System;

namespace NomadLogic.iBrandDesktop.Util
{
    /// <summary>
    /// Accesseur statique. Très mauvais design. À enlever dès que possible.
    /// </summary>
    [Obsolete]
    public static class ProjectAccessor
    {
        private static Project _project;

        public static Project Project
        {
            get
            {
                return _project;
            }

            set
            {
                _project = value;
            }
        }
    }
}
