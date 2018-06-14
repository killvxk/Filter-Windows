using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Citadel.IPC;
using Citadel.IPC.Messages;
using Citadel.Platform.Common.IPC;
using Citadel.Platform.Common.Util;

namespace Filter.Platform.Mac.IPC
{
    public class MacIPCServer : IPCServer, IDisposable
    {
        private IntPtr m_serverHandle;

        public MacIPCServer()
        {
            m_serverHandle = NativeIPCServerImpl.CreateIPCServer("org.cloudveil.FilterServiceProvider.server", OnMessageReceived, OnClientConnected, OnClientDisconnected);
        }

        private void OnClientConnected()
        {
            base.ClientConnected?.Invoke();
        }

        private void OnClientDisconnected()
        {
            base.ClientDisconnected?.Invoke();
        }

        private void OnMessageReceived(IntPtr incomingData, int dataLength)
        {
            byte[] data = new byte[dataLength];
            Marshal.Copy(incomingData, data, 0, dataLength);

            // First, let's take the data and transform it into a C# type if possible.
            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream(data))
            {
                try
                {
                    object msgObj = formatter.Deserialize(stream);

                    BaseMessage msg = msgObj as BaseMessage;

                    OnClientMessage(msg);
                }
                catch(Exception ex)
                {
                    LoggerUtil.RecursivelyLogException(m_logger, ex);
                }
            }
        }

        protected override void PushMessage(BaseMessage msg)
        {
            if (m_waitingForAuth)
            {
                // We'll only allow auth messages through until authentication has
                // been confirmed.
                if (msg.GetType() == typeof(AuthenticationMessage))
                {
                    PushMessageInternal(msg);
                }
                else if (msg.GetType() == typeof(RelaxedPolicyMessage))
                {
                    PushMessageInternal(msg);
                }
                else if (msg.GetType() == typeof(NotifyBlockActionMessage))
                {
                    PushMessageInternal(msg);
                }
            }
            else
            {
                PushMessageInternal(msg);
            }
        }

        protected void PushMessageInternal(BaseMessage msg)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using(MemoryStream stream = new MemoryStream())
            {
                try {
                    formatter.Serialize(stream, msg);

                    byte[] msgArray = stream.ToArray();

                    NativeIPCServerImpl.SendToAll(m_serverHandle, msgArray, msgArray.Length);
                }
                catch(Exception e)
                {
                    LoggerUtil.RecursivelyLogException(m_logger, e);
                }
            }
        }

        bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    NativeIPCServerImpl.Release(m_serverHandle);
                    m_serverHandle = IntPtr.Zero;
                }
            }
        }

        public override void Dispose()
        {
            Dispose(true);
        }
    }
}
