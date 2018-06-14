using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Citadel.IPC.Messages;
using Citadel.Platform.Common.IPC;
using Citadel.Platform.Common.Util;

namespace Filter.Platform.Mac.IPC
{
    public class MacIPCClient : IPCClient
    {
        private IntPtr m_clientHandle;

        public MacIPCClient(bool autoReconnect = false) : base(autoReconnect)
        {
            // TODO: Implement auto-reconnect on IPC on macOS.

            m_clientHandle = NativeIPCClientImpl.CreateIPCClient(OnMessage, OnConnected);

            NativeIPCClientImpl.Connect(m_clientHandle, "org.cloudveil.FilterServiceProvider.server");
        }

        public static IPCClient InitDefault()
        {
            Default = new MacIPCClient(true);
            return Default;
        }

        public override void WaitForConnection()
        {
            while(!NativeIPCClientImpl.IsConnected(m_clientHandle))
            {
                Thread.Sleep(2);
            }
        }

        private void onClientError(Exception exception)
        {
            LoggerUtil.RecursivelyLogException(m_logger, exception);
        }

        private void OnConnected()
        {
            ConnectedToServer?.Invoke();
        }

        private void OnMessage(IntPtr incomingData, int dataLength)
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

                    base.OnServerMessage(msg);
                }
                catch(Exception e)
                {
                    LoggerUtil.RecursivelyLogException(m_logger, e);
                }
            }
        }

        protected override void PushMessage(BaseMessage msg, GenericReplyHandler replyHandler = null)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, msg);

                byte[] array = ms.ToArray();

                NativeIPCClientImpl.Send(m_clientHandle, array, array.Length);
            }

            if (replyHandler != null)
            {
                if (Default != null)
                {
                    Default.IpcQueue.AddMessage(msg, replyHandler);
                }
                else
                {
                    m_ipcQueue.AddMessage(msg, replyHandler);
                }
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
                    if (m_clientHandle != null)
                    {
                        NativeIPCClientImpl.Disconnect(m_clientHandle);
                        NativeIPCClientImpl.Release(m_clientHandle);
                        m_clientHandle = IntPtr.Zero;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~IPCClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
