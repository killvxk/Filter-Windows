using NamedPipeWrapper;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CloudVeilInstallerUI.IPC
{
    public abstract class IPCCommunicator
    {
        public IPCCommunicator()
        {
            MessageReceived += OnMessage;
        }

        Dictionary<string, object> variableObjects = new Dictionary<string, object>();

        public Dictionary<string, object> VariableObjects => variableObjects;

        public void RegisterObject(string varName, object o)
        {
            variableObjects[varName] = o;
        }

        public object GetObject(string varName)
        {
            object o;
            if(variableObjects.TryGetValue(varName, out o))
            {
                return o;
            } else
            {
                return null;
            }
        }

        public void Set(string varName, string property, object value)
        {
            PushMessage(new Message()
            {
                Command = Command.Set,
                VariableName = varName,
                Property = property,
                Data = value
            });
        }

        public Task<object> Call(string varName, string method, object[] parameters)
        {
            object ret = null;

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            Guid replyId = Guid.NewGuid();

            ConnectionMessageEventHandler<Message, Message> fn = null;

            fn = (connection, msg) =>
            {
                if (msg.Id.Equals(replyId))
                {
                    MessageReceived -= fn;
                    if(!tcs.TrySetResult(msg.Data))
                    {
                        tcs.TrySetException(new Exception("Failed to set TaskCompletionSource result."));
                    }
                }
            };

            MessageReceived += fn;

            PushMessage(new Message(replyId)
            {
                Command = Command.Call,
                VariableName = varName,
                Property = method,
                Data = parameters
            });

            return tcs.Task;
        }

        public Task<object> Get(string varName, string property)
        {
            var tcs = new TaskCompletionSource<object>();

            Guid replyId = Guid.NewGuid();

            ConnectionMessageEventHandler<Message, Message> fn = null;

            fn = (connection, msg) =>
            {
                if (msg.Id.Equals(replyId))
                {
                    MessageReceived -= fn;
                    if(!tcs.TrySetResult(msg.Data))
                    {
                        Console.WriteLine("Failed to set TaskCompletionSource.Result");
                    }
                }
            };

            MessageReceived += fn;

            PushMessage(new Message(replyId)
            {
                Command = Command.Get,
                VariableName = varName,
                Property = property,
                Data = null
            });

            return tcs.Task;
        }

        public abstract void PushMessage(Message message);
        
        public event ConnectionMessageEventHandler<Message, Message> MessageReceived;

        private static void Error(NamedPipeConnection<Message, Message> conn)
        {
            conn.PushMessage(new Message()
            {
                Command = Command.Error,
                Data = null
            });
        }

        protected void OnMessageReceived(NamedPipeConnection<Message, Message> connection, Message message)
        {
            MessageReceived?.Invoke(connection, message);
        }

        protected void OnMessage(NamedPipeConnection<Message, Message> connection, Message message)
        {
            Console.WriteLine("OnMessage Received {0}, {1}, {2}", message.Command, message.Data?.GetType()?.Name, message.Property);
            switch (message.Command)
            {
                case Command.Set:
                    {
                        object obj;

                        if (!variableObjects.TryGetValue(message.VariableName, out obj))
                        {
                            Error(connection);
                            return;
                        }

                        if (obj == null)
                        {
                            Error(connection);
                            return;
                        }

                        Type objType = obj.GetType();

                        var propInfo = objType.GetProperty(message.Property);

                        if (propInfo == null)
                        {
                            Error(connection);
                            return;
                        }

                        try
                        {
                            propInfo.SetValue(obj, message.Data);
                        }
                        catch (Exception ex)
                        {
                            Error(connection);
                        }
                    }
                    break;

                case Command.Get:
                    {
                        object obj;

                        if (!variableObjects.TryGetValue(message.VariableName, out obj))
                        {
                            Error(connection);
                            return;
                        }

                        if (obj == null)
                        {
                            Error(connection);
                            return;
                        }

                        Type objType = obj.GetType();

                        var propInfo = objType.GetProperty(message.Property);

                        if (propInfo == null)
                        {
                            Error(connection);
                            return;
                        }

                        try
                        {
                            object ret = propInfo.GetValue(obj);

                            connection.PushMessage(new Message(message.Id)
                            {
                                Command = Command.GetResponse,
                                Property = message.Property,
                                VariableName = message.VariableName,
                                Data = ret
                            });
                        }
                        catch (Exception ex)
                        {
                            Error(connection);
                        }
                    }
                    break;

                case Command.Call:
                    {
                        object obj;

                        if (!variableObjects.TryGetValue(message.VariableName, out obj))
                        {
                            Error(connection);
                            return;
                        }

                        if (obj == null)
                        {
                            Error(connection);
                            return;
                        }

                        Type objType = obj.GetType();

                        var methodInfo = objType.GetMethod(message.Property);

                        if (methodInfo == null)
                        {
                            Error(connection);
                            return;
                        }

                        try
                        {
                            object ret = methodInfo.Invoke(obj, message.Data as object[]);
                            connection.PushMessage(new Message()
                            {
                                Command = Command.Response,
                                Property = message.Property,
                                VariableName = message.VariableName,
                                Data = ret
                            });
                        }
                        catch (Exception ex)
                        {
                            Error(connection);
                        }
                    }
                    break;

            }
        }

    }

    public class UpdateIPCServer : IPCCommunicator
    {
        NamedPipeServer<Message> server;

        public static UpdateIPCServer Default { get; private set; }

        public UpdateIPCServer()
        {
            this.server.ClientConnected += Server_ClientConnected;
            this.server.ClientDisconnected += Server_ClientDisconnected;
        }

        public event EventHandler ClientConnected;
        public event EventHandler ClientDisconnected;

        private void Server_ClientConnected(NamedPipeConnection<Message, Message> connection)
        {
            ClientConnected?.Invoke(this, new EventArgs());
        }

        private void Server_ClientDisconnected(NamedPipeConnection<Message, Message> connection)
        {
            ClientDisconnected?.Invoke(this, new EventArgs());
        }

        private static PipeSecurity GetSecurityForChannel()
        {
            PipeSecurity pipeSecurity = new PipeSecurity();

            var permissions = PipeAccessRights.CreateNewInstance | PipeAccessRights.Read | PipeAccessRights.Synchronize | PipeAccessRights.Write;

            var authUsersSid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            var authUsersAcct = authUsersSid.Translate(typeof(NTAccount));
            pipeSecurity.SetAccessRule(new PipeAccessRule(authUsersAcct, permissions, AccessControlType.Allow));

            return pipeSecurity;
        }

        public static void Init(string pipeName)
        {
            Default = new UpdateIPCServer(pipeName);
        }

        public UpdateIPCServer(string pipeName)
        {
            server = new NamedPipeServer<Message>(pipeName, GetSecurityForChannel());

            server.ClientMessage += OnMessageReceived;
        }

        public void Start() => server.Start();

        public override void PushMessage(Message message)
        {
            server.PushMessage(message);
        }
    }
}
