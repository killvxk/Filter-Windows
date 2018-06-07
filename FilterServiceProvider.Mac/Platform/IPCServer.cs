using System;
using Citadel.IPC.Messages;
using Citadel.Platform.Common.IPC;
using Citadel.Platform.Common.Types;

namespace FilterServiceProvider.Mac.Platform
{
    public class IPCServer : IIPCServer
    {
        public IPCServer()
        {
        }

        public bool WaitingForAuth { get; private set; }

        public RelaxPolicyRequestHander RelaxedPolicyRequested { get; set; }
        public DeactivationRequestHandler DeactivationRequested { get; set; }
        public AuthenticationRequestHandler AttemptAuthentication { get; set; }
        public ServerGenericParameterlessHandler ClientConnected { get; set; }
        public ServerGenericParameterlessHandler ClientDisconnected { get; set; }
        public StateChangeHandler ClientServerStateQueried { get; set; }
        public ServerGenericParameterlessHandler ClientAcceptedPendingUpdate { get; set; }
        public BlockActionReportHandler ClientRequestsBlockActionReview { get; set; }
        public RequestConfigUpdateHandler RequestConfigUpdate { get; set; }
        public RequestCaptivePortalDetectionHandler RequestCaptivePortalDetection { get; set; }

        public void NotifyApplicationUpdateAvailable(ServerUpdateQueryMessage message)
        {
            // TODO: Implement
        }

        public void NotifyAuthenticationStatus(AuthenticationAction action, AuthenticationResultObject authenticationResult = null)
        {
            // TODO: Implement
        }

        public void NotifyBlockAction(BlockType type, Uri resource, string category, string rule = null)
        {
            // TODO: Implement
        }

        public void NotifyConfigurationUpdate(ConfigUpdateResult result, Guid replyToId)
        {
            // TODO: Implement
        }

        public void NotifyCooldownEnforced(TimeSpan cooldownPeriod)
        {
            // TODO: Implement
        }

        public void NotifyRelaxedPolicyChange(int numPoliciesAvailable, TimeSpan policyDuration, RelaxedPolicyStatus status)
        {
            // TODO: Implement
        }

        public void NotifyStatus(FilterStatus status)
        {
            // TODO: Implement
        }

        public void NotifyUpdating()
        {
            // TODO: Implement
        }

        public void SendCaptivePortalState(bool captivePortalState, bool isCaptivePortalActive)
        {
            // TODO: Implement
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IPCServer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
