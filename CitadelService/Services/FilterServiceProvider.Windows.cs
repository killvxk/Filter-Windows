#if WINDOWS
using System;
using System.Threading;
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
                Thread thread = new Thread(OnStartup);
                thread.Start();

                //OnStartup();
            }
            catch (Exception e)
            {
                // Critical failure.
                try
                {
                    EventLog.CreateEventSource("FilterServiceProvider", "Application");
                    EventLog.WriteEntry("FilterServiceProvider", $"Exception occurred before logger was bootstrapped: {e.ToString()}");
                }
                catch (Exception e2)
                {
                    File.AppendAllText(@"C:\FilterServiceProvider.FatalCrashLog.log", $"Fatal crash.\r\n{e.ToString()}\r\n{e2.ToString()}");
                }

                //LoggerUtil.RecursivelyLogException(m_logger, e);
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
#endif