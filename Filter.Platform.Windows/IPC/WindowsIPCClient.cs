using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Citadel.IPC.Messages;
using Citadel.Platform.Common.IPC;
using Citadel.Platform.Common.Util;

namespace Filter.Platform.Windows.IPC
{
    public class WindowsIPCClient : IPCClient
    {
        public WindowsIPCClient(bool autoReconnect = false) : base(autoReconnect)
        {
            m_client = new NamedPipeClient<BaseMessage>(channel);

            m_client.Connected += OnConnected;
            m_client.Disconnected += OnDisconnected;
            m_client.ServerMessage += OnPipeConnectionMessage;
            m_client.AutoReconnect = autoReconnect;

            m_client.Error += onClientError;

            m_client.Start();
        }

        public static IPCClient InitDefault()
        {
            Default = new WindowsIPCClient(true);
            return Default;
        }

        private NamedPipeWrapper<BaseMessage, BaseMessage> m_client;

        private void onClientError(Exception exception)
        {
            LoggerUtil.RecursivelyLogException(m_logger, exception);
        }

        private void OnConnected(NamedPipeConnection<BaseMessage, BaseMessage> connection)
        {
            ConnectedToServer?.Invoke();
        }

        private void OnDisconnected(NamedPipeConnection<BaseMessage, BaseMessage> connection)
        {
            DisconnectedFromServer?.Invoke();
        }

        private void OnPipeConnectionMessage(NamedPipeConnection<BaseMessage, BaseMessage> connection, BaseMessage message)
        {
            base.OnServerMessage(message);
        }

        protected override void PushMessage(BaseMessage msg, GenericReplyHandler replyHandler = null)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, msg);
            }

            m_client.PushMessage(msg);

            if (replyHandler != null)
            {
                if (Default != null)
                {
                    Default.m_ipcQueue.AddMessage(msg, replyHandler);
                }
                else
                {
                    m_ipcQueue.AddMessage(msg, replyHandler);
                }
            }
        }

        public override void WaitForConnection()
        {
            m_client.WaitForConnection();
        }
    }
}
