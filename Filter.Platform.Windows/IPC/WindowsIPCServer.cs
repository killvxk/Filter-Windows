using System;
using Citadel.IPC;
using Citadel.Platform.Common.IPC;

namespace Filter.Platform.Windows.IPC
{
    public class WindowsIPCServer : IPCServer
    {
        public WindowsIPCServer()
        {
            var security = GetSecurityForChannel();

            var channel = string.Format("{0}.{1}", nameof(Citadel.IPC), FingerPrint.Value).ToLower();

        }

        /// <summary>
        /// Actual named pipe server wrapper. 
        /// </summary>
        private NamedPipeServer<BaseMessage> m_server;

        private void M_server_Error(Exception exception)
        {
            LoggerUtil.RecursivelyLogException(m_logger, exception);
        }

        private void OnClientConnected(NamedPipeConnection<BaseMessage, BaseMessage> connection)
        {
            m_logger.Debug("Client connected.");
            ClientConnected?.Invoke();
        }

        private void OnClientDisconnected(NamedPipeConnection<BaseMessage, BaseMessage> connection)
        {
            m_logger.Debug("Client disconnected.");
            ClientDisconnected?.Invoke();
            connection.ReceiveMessage -= OnClientMessage;
        }

        private void OnMessageReceived(NamedPipeConnection<BaseMessage, BaseMessage> connection, BaseMessage message)
        {
            OnClientMessage(message);
        }

        /// <summary>
        /// Gets a security descriptor that will permit non-elevated clients to connect to the server. 
        /// </summary>
        /// <returns>
        /// A security descriptor that will permit non-elevated clients to connect to the server. 
        /// </returns>
        private static PipeSecurity GetSecurityForChannel()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();

            var permissions = PipeAccessRights.CreateNewInstance | PipeAccessRights.Read | PipeAccessRights.Synchronize | PipeAccessRights.Write;

            var authUsersSid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            var authUsersAcct = authUsersSid.Translate(typeof(NTAccount));
            pipeSecurity.SetAccessRule(new PipeAccessRule(authUsersAcct, permissions, AccessControlType.Allow));

            return pipeSecurity;
        }

        protected override void PushMessage(BaseMessage msg)
        {
            if (m_waitingForAuth)
            {
                // We'll only allow auth messages through until authentication has
                // been confirmed.
                if (msg.GetType() == typeof(AuthenticationMessage))
                {
                    m_server.PushMessage(msg);
                }
                else if (msg.GetType() == typeof(RelaxedPolicyMessage))
                {
                    m_server.PushMessage(msg);
                }
                else if (msg.GetType() == typeof(NotifyBlockActionMessage))
                {
                    m_server.PushMessage(msg);
                }
            }
            else
            {
                m_server.PushMessage(msg);
            }
        }


        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (m_server != null)
                    {
#if CLIFTON
                        m_server.Close();
#else
                        m_server.Stop();
#endif
                        m_server = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free
        //       unmanaged resources. ~IPCServer() { // Do not change this code. Put cleanup code in
        // Dispose(bool disposing) above. Dispose(false); }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
