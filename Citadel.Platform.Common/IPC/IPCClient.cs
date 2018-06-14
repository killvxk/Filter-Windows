/*
* Copyright © 2017 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using Citadel.Core.Windows.Util;
using Citadel.IPC.Messages;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Citadel.Platform.Common;
using Citadel.Platform.Common.Util;

namespace Citadel.Platform.Common.IPC
{   

    public abstract class IPCClient : IIPCClient, IDisposable
    {

        protected IPCMessageTracker m_ipcQueue;

        public IPCMessageTracker IpcQueue { get { return m_ipcQueue; }}

        public ClientGenericParameterlessHandler ConnectedToServer;

        public ClientGenericParameterlessHandler DisconnectedFromServer;

        public StateChangeHandler StateChanged;

        public AuthenticationResultHandler AuthenticationResultReceived;

        private AuthenticationMessage AuthMessage;

        public DeactivationRequestResultHandler DeactivationResultReceived;

        public BlockActionReportHandler BlockActionReceived;

        public ClientGenericParameterlessHandler RelaxedPolicyExpired;

        public RelaxedPolicyInfoReceivedHandler RelaxedPolicyInfoReceived;

        public ClientToClientMessageHandler ClientToClientCommandReceived;

        public ServerUpdateRequestHandler ServerAppUpdateRequestReceived;

        public CaptivePortalDetectionHandler CaptivePortalDetectionReceived;

        public ClientGenericParameterlessHandler ServerUpdateStarting;

        /// <summary>
        /// Our logger.
        /// </summary>
        protected readonly IAppLogger m_logger;

        /// <summary>
        /// All message handlers get added to this IPC client as it will stick around and handle all the incoming messages.
        /// </summary>
        public static IPCClient Default { get; set; }

        public IPCClient(bool autoReconnect = false)
        {
            m_logger = LoggerUtil.GetAppWideLogger();

            m_logger.Info("Creating client.");

            m_ipcQueue = new IPCMessageTracker();
        }

        public abstract void WaitForConnection();

        public AuthenticationMessage GetAuthMessage()
        {
            return AuthMessage;
        }

        protected void OnServerMessage(BaseMessage message)
        {
            // This is so gross, but unfortuantely we can't just switch on a type.
            // We can come up with a nice mapping system so we can do a switch,
            // but this can wait.

            if(Default.m_ipcQueue.HandleMessage(message))
            {
                return;
            }

            if(m_ipcQueue.HandleMessage(message))
            {
                return;
            }

            m_logger.Debug("Got IPC message from server.");

            var msgRealType = message.GetType();

            if(msgRealType == typeof(AuthenticationMessage))
            {
                AuthMessage = (AuthenticationMessage)message;
                m_logger.Debug("Server message is {0}", nameof(AuthenticationMessage));
                var cast = (AuthenticationMessage)message;
                if(cast != null)
                {   
                    AuthenticationResultReceived?.Invoke(cast);
                }
            }
            else if(msgRealType == typeof(DeactivationMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(DeactivationMessage));
                var cast = (DeactivationMessage)message;
                if(cast != null)
                {   
                    DeactivationResultReceived?.Invoke(cast.Command);
                }
            }
            else if(msgRealType == typeof(FilterStatusMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(FilterStatusMessage));
                var cast = (FilterStatusMessage)message;
                if(cast != null)
                {   
                    StateChanged?.Invoke(new StateChangeEventArgs(cast));
                }
            }
            else if(msgRealType == typeof(NotifyBlockActionMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(NotifyBlockActionMessage));
                var cast = (NotifyBlockActionMessage)message;
                if(cast != null)
                {   
                    BlockActionReceived?.Invoke(cast);
                }
            }
            else if(msgRealType == typeof(RelaxedPolicyMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(RelaxedPolicyMessage));
                var cast = (RelaxedPolicyMessage)message;
                if(cast != null)
                {   
                    switch(cast.Command)
                    {
                        case RelaxedPolicyCommand.Info:
                        {
                            RelaxedPolicyInfoReceived?.Invoke(cast);
                        }
                        break;

                        case RelaxedPolicyCommand.Expired:
                        {
                            RelaxedPolicyExpired?.Invoke();
                        }
                        break;
                    }
                }
            }
            else if(msgRealType == typeof(ClientToClientMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(ClientToClientMessage));
                var cast = (ClientToClientMessage)message;
                if(cast != null)
                {   
                    ClientToClientCommandReceived?.Invoke(cast);
                }
            }
            else if(msgRealType == typeof(ServerUpdateQueryMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(ServerUpdateQueryMessage));
                var cast = (ServerUpdateQueryMessage)message;
                if(cast != null)
                {
                    ServerAppUpdateRequestReceived?.Invoke(cast);
                }
            }
            else if(msgRealType == typeof(ServerUpdateNotificationMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(ServerUpdateNotificationMessage));
                var cast = (ServerUpdateNotificationMessage)message;
                if(cast != null)
                {
                    ServerUpdateStarting?.Invoke();
                }
            }
            else if(msgRealType == typeof(CaptivePortalDetectionMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(CaptivePortalDetectionMessage));
                var cast = (CaptivePortalDetectionMessage)message;
                if(cast != null)
                {
                    CaptivePortalDetectionReceived?.Invoke(cast);
                }
            }
            else
            {
                // Unknown type.
            }
        }

        /// <summary>
        /// Requests that the server deactivate the filtering service and shut down.
        /// </summary>
        public void RequestDeactivation()
        {
            var msg = new DeactivationMessage(DeactivationCommand.Requested);
            PushMessage(msg);
        }

        /// <summary>
        /// Sends an IPC message to the server notifying that the client has requested a block action
        /// to be reviewed.
        /// </summary>
        /// <param name="category">
        /// The category of the rule that caused the block action.
        /// </param>
        /// <param name="fullRequestUrl">
        /// The full URL that was blocked by the rule.
        /// </param>
        public void RequestBlockActionReview(string category, string fullRequestUrl)
        {
            var msg = new BlockActionReviewRequestMessage(category, fullRequestUrl);
            PushMessage(msg);
        }

        /// <summary>
        /// Requests the current status from the IPC server.
        /// </summary>
        public void RequestStatusRefresh()
        {
            var msg = new FilterStatusMessage(FilterStatus.Query);
            PushMessage(msg);
        }

        /// <summary>
        /// Sends credentials to the server to attempt authentication. 
        /// </summary>
        /// <param name="username">
        /// Username to authorize with.
        /// </param>
        /// <param name="password">
        /// Password to authorize with.
        /// </param>
        public void AttemptAuthentication(string username, SecureString password)
        {
            var msg = new AuthenticationMessage(AuthenticationAction.Requested, username, password);

            var logger = LoggerUtil.GetAppWideLogger();

            try
            {
                PushMessage(msg);
            }
            catch(Exception e)
            {
                LoggerUtil.RecursivelyLogException(logger, e);
            }
        }

        public void RequestPrimaryClientShowUI()
        {
            var msg = new ClientToClientMessage(ClientToClientCommand.ShowYourself);
            PushMessage(msg);
        }

        public void RequestRelaxedPolicy()
        {
            var msg = new RelaxedPolicyMessage(RelaxedPolicyCommand.Requested);
            PushMessage(msg);
        }

        public void RelinquishRelaxedPolicy()
        {
            var msg = new RelaxedPolicyMessage(RelaxedPolicyCommand.Relinquished);
            PushMessage(msg);
        }

        public void NotifyAcceptUpdateRequest()
        {
            var msg = new ClientUpdateResponseMessage(true);
            PushMessage(msg);
        }

        public void RequestConfigUpdate(Action<NotifyConfigUpdateMessage> replyHandler)
        {
            var msg = new RequestConfigUpdateMessage();
            PushMessage(msg, (reply) =>
            {
                if (reply.GetType() == typeof(NotifyConfigUpdateMessage))
                {
                    replyHandler((NotifyConfigUpdateMessage)reply);
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        protected abstract void PushMessage(BaseMessage msg, GenericReplyHandler replyHandler = null);

        public abstract void Dispose();
    }
}
