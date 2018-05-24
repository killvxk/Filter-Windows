using System;
using Citadel.Core.Windows.Util;

namespace CitadelService.Services
{
    public partial class FilterServiceProvider
    {
        #region Windows Service API

        public bool Start()
        {
            try
            {
                LogTime("Starting FilterServiceProvider");
                OnStartup();
            }
            catch (Exception e)
            {
                // Critical failure.
                LoggerUtil.RecursivelyLogException(m_logger, e);
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            // We always return false because we don't let anyone tell us that we're going to stop.
            return false;
        }

        public bool Shutdown()
        {
            // Called on a shutdown event.
            Environment.Exit((int)ExitCodes.ShutdownWithSafeguards);
            return true;
        }

        public void OnSessionChanged()
        {
            ReviveGuiForCurrentUser(true);
        }

        #endregion Windows Service API
    }
}
