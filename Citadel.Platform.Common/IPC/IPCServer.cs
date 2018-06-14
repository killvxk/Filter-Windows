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
using System.IO.Pipes;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using Citadel.Platform.Common;
using Citadel.Platform.Common.IPC;
using Citadel.Platform.Common.Util;
using Citadel.Platform.Common.Types;

namespace Citadel.IPC
{
    
    /// <summary>
    /// The IPC server class is meant to be used with a session 0 isolated process, more specifically
    /// a Windows service. This class handles requests from clients (GUI) and responds accordingly.
    /// </summary>
    public abstract class IPCServer : IDisposable, IIPCServer
    {
        // XXX FIXME Currently not used in IPCServer.
        private IPCMessageTracker m_ipcQueue;

        /// <summary>
        /// Delegate to be called when a client requests a relaxed policy. 
        /// </summary>
        public RelaxPolicyRequestHander RelaxedPolicyRequested { get; set; }

        /// <summary>
        /// Delegate to be called when a client requests filter deactivation. 
        /// </summary>
        public DeactivationRequestHandler DeactivationRequested { get; set; }

        /// <summary>
        /// Delegate to be called when a client attempts to authenticate. 
        /// </summary>
        public AuthenticationRequestHandler AttemptAuthentication { get; set; }

        /// <summary>
        /// Delegate to be called when a client connects. 
        /// </summary>
        public ServerGenericParameterlessHandler ClientConnected { get; set; }

        /// <summary>
        /// Delegate to be called when a client disconnects. 
        /// </summary>
        public ServerGenericParameterlessHandler ClientDisconnected { get; set; }

        /// <summary>
        /// Delegate to be called when a client is querying the state of the filter. 
        /// </summary>
        public StateChangeHandler ClientServerStateQueried { get; set; }

        /// <summary>
        /// Delegate to be called when a client has responded accepting a pending
        /// application update.
        /// </summary>
        public ServerGenericParameterlessHandler ClientAcceptedPendingUpdate { get; set; }

        /// <summary>
        /// Delegate to be called when a client has submitted a block action for
        /// review.
        /// </summary>
        public BlockActionReportHandler ClientRequestsBlockActionReview { get; set; }

        /// <summary>
        /// Delegate to be called when a client is requesting a configuration/ruleset update.
        /// </summary>
        public RequestConfigUpdateHandler RequestConfigUpdate { get; set; }

        /// <summary>
        /// Delegate to be called when a client is requesting a captive portal state.
        /// </summary>
        public RequestCaptivePortalDetectionHandler RequestCaptivePortalDetection { get; set; }

        /// <summary>
        /// Our logger. 
        /// </summary>
        protected readonly IAppLogger m_logger;

        public bool WaitingForAuth
        {
            get
            {
                return m_waitingForAuth;
            }
        }

        protected volatile bool m_waitingForAuth = false;

        /// <summary>
        /// Constructs a new named pipe server for IPC, with a channel name derived from the class
        /// namespace and the current machine's digital fingerprint.
        /// </summary>
        public IPCServer()
        {
            m_logger = LoggerUtil.GetAppWideLogger();

            m_ipcQueue = new IPCMessageTracker();

            m_logger.Info("IPC Server started.");
        }



        /// <summary>
        /// Handles a received client message. 
        /// </summary>
        /// <param name="connection">
        /// The connection over which the message was received. 
        /// </param>
        /// <param name="message">
        /// The client's message to us. 
        /// </param>
        protected void OnClientMessage(BaseMessage message)
        {
            // This is so gross, but unfortuantely we can't just switch on a type. We can come up
            // with a nice mapping system so we can do a switch, but this can wait.

            m_logger.Debug("Got IPC message from client.");

            var msgRealType = message.GetType();

            if(msgRealType == typeof(Messages.AuthenticationMessage))
            {
                m_logger.Debug("Client message is {0}", nameof(Messages.AuthenticationMessage));
                var cast = (Messages.AuthenticationMessage)message;

                if(cast != null)
                {
                    if(string.IsNullOrEmpty(cast.Username) || string.IsNullOrWhiteSpace(cast.Username))
                    {
                        PushMessage(new AuthenticationMessage(AuthenticationAction.InvalidInput));
                        return;
                    }

                    if (cast.Password == null || cast.Password.Length <= 0)
                    {
                        PushMessage(new AuthenticationMessage(AuthenticationAction.InvalidInput));
                        return;
                    }

                    var args = new AuthenticationRequestArgs(cast);

                    AttemptAuthentication?.Invoke(args);
                }
            }
            else if (msgRealType == typeof(Messages.DeactivationMessage))
            {
                m_logger.Debug("Client message is {0}", nameof(Messages.DeactivationMessage));

                var cast = (Messages.DeactivationMessage)message;

                if (cast != null && cast.Command == DeactivationCommand.Requested)
                {
                    var args = new DeactivationRequestEventArgs();

                    // This fills args.DeactivationCommand.
                    DeactivationRequested?.Invoke(args);

                    PushMessage(new DeactivationMessage(args.DeactivationCommand));
                }
            }
            else if (msgRealType == typeof(RelaxedPolicyMessage))
            {
                m_logger.Debug("Client message is {0}", nameof(RelaxedPolicyMessage));

                var cast = (RelaxedPolicyMessage)message;

                if (cast != null)
                {
                    var args = new RelaxedPolicyEventArgs(cast);
                    RelaxedPolicyRequested?.Invoke(args);
                }
            }
            else if (msgRealType == typeof(Messages.ClientToClientMessage))
            {
                m_logger.Debug("Client message is {0}", nameof(Messages.ClientToClientMessage));

                var cast = (Messages.ClientToClientMessage)message;

                if (cast != null)
                {
                    // Just relay this message to all clients.
                    PushMessage(cast);
                }
            }
            else if (msgRealType == typeof(Messages.FilterStatusMessage))
            {
                m_logger.Debug("Client message is {0}", nameof(Messages.FilterStatusMessage));

                var cast = (Messages.FilterStatusMessage)message;

                if (cast != null)
                {
                    ClientServerStateQueried?.Invoke(new StateChangeEventArgs(cast));
                }
            }
            else if (msgRealType == typeof(Messages.ClientUpdateResponseMessage))
            {
                m_logger.Debug("Client message is {0}", nameof(Messages.ClientUpdateResponseMessage));

                var cast = (Messages.ClientUpdateResponseMessage)message;

                if (cast != null)
                {
                    if (cast.Accepted)
                    {
                        m_logger.Debug("Client has accepted update.");
                        ClientAcceptedPendingUpdate?.Invoke();
                    }
                }
            }
            else if (msgRealType == typeof(Messages.BlockActionReviewRequestMessage))
            {
                m_logger.Debug("Client message is {0}", nameof(Messages.BlockActionReviewRequestMessage));

                var cast = (Messages.BlockActionReviewRequestMessage)message;

                if (cast != null)
                {
                    Uri output;
                    if (Uri.TryCreate(cast.FullRequestUrl, UriKind.Absolute, out output))
                    {
                        // Here we'll just recycle the block action message and handler.
                        ClientRequestsBlockActionReview?.Invoke(new NotifyBlockActionMessage(BlockType.OtherContentClassification, output, string.Empty, cast.CategoryName));
                    }
                    else
                    {
                        m_logger.Info("Failed to create absolute URI for string \"{0}\".", cast.FullRequestUrl);
                    }
                }
            }
            else if (msgRealType == typeof(Messages.RequestConfigUpdateMessage))
            {
                m_logger.Debug("Client message is {0}", nameof(Messages.RequestConfigUpdateMessage));

                var cast = (Messages.RequestConfigUpdateMessage)message;

                if (cast != null)
                {
                    RequestConfigUpdate?.Invoke(cast);
                }
            }
            else if (msgRealType == typeof(Messages.CaptivePortalDetectionMessage))
            {
                m_logger.Debug("Server message is {0}", nameof(Messages.CaptivePortalDetectionMessage));
                var cast = (Messages.CaptivePortalDetectionMessage)message;
                if (cast != null)
                {
                    RequestCaptivePortalDetection?.Invoke(cast);
                }
            }
            else
            {
                // Unknown type.
            }
        }

        /// <summary>
        /// Notifies all clients of the supplied relaxed policy.Debugrmation. 
        /// </summary>
        /// <param name="numPoliciesAvailable">
        /// The number of relaxed policy uses available. 
        /// </param>
        /// <param name="policyDuration">
        /// The duration of the relaxed policies. 
        /// </param>
        /// <param name="isActive">
        /// Whether or not the relaxed policy is currently active.
        /// </param>
        /// <param name="command">
        /// The relaxed policy command which caused this notification to happen.
        /// If == RelaxedPolicyCommand.Info, ignore.
        /// </param>
        public void NotifyRelaxedPolicyChange(int numPoliciesAvailable, TimeSpan policyDuration, RelaxedPolicyStatus status)
        {
            var nfo = new RelaxedPolicyInfo(policyDuration, numPoliciesAvailable, status);
            var msg = new RelaxedPolicyMessage(RelaxedPolicyCommand.Info, nfo);
            PushMessage(msg);
        }

        /// <summary>
        /// Notifies clients of the supplied status change. 
        /// </summary>
        /// <param name="status">
        /// The status to send to all clients. 
        /// </param>
        public void NotifyStatus(FilterStatus status)
        {
            var msg = new FilterStatusMessage(status);
            PushMessage(msg);
        }

        public void NotifyCooldownEnforced(TimeSpan cooldownPeriod)
        {
            var msg = new FilterStatusMessage(cooldownPeriod);
            PushMessage(msg);
        }

        /// <summary>
        /// Notifies clients of a block action. 
        /// </summary>
        /// <param name="type">
        /// The type of block action, AKA the cause. 
        /// </param>
        /// <param name="resource">
        /// The absolute URI of the requested resource that caused the blocked network connection. 
        /// </param>
        /// <param name="category">
        /// The category that the matching rule belongs to. 
        /// </param>
        /// <param name="rule">
        /// The matching rule, if applicable. Defaults to null; 
        /// </param>
        public void NotifyBlockAction(BlockType type, Uri resource, string category, string rule = null)
        {
            var msg = new NotifyBlockActionMessage(type, resource, rule, category);
            PushMessage(msg);
        }

        /// <summary>
        /// Notifies clients of the current authentication state. 
        /// </summary>
        /// <param name="action">
        /// The authentication command which reflects the current auth state. 
        /// </param>
        public void NotifyAuthenticationStatus(AuthenticationAction action, AuthenticationResultObject authenticationResult = null)
        {
            // KF - I edited this function to take two arguments instead of one and then refactored all the code that calls it to pass in an AuthenticationResultObject
            switch (m_waitingForAuth)
            {
                case true:
                    {
                        m_waitingForAuth = action == AuthenticationAction.Authenticated ? false : true;
                    }
                    break;

                case false:
                    {
                        m_waitingForAuth = action == AuthenticationAction.Required;
                    }
                    break;
            }


            var authResult = new AuthenticationResultObject();

            if (authenticationResult != null)
            {
                authResult = authenticationResult;
            }

            var msg = new AuthenticationMessage(action, authResult); // KF - Also added a constructor to AuthenticationMessage);
            PushMessage(msg);
        }

        /// <summary>
        /// Notifies all clients of an available update. 
        /// </summary>
        /// <param name="message">
        /// The update message.
        /// </param>
        public void NotifyApplicationUpdateAvailable(ServerUpdateQueryMessage message)
        {
            PushMessage(message);
        }

        /// <summary>
        /// Notifies all clients that the server is updating itself.
        /// </summary>
        public void NotifyUpdating()
        {
            var msg = new ServerUpdateNotificationMessage();
            PushMessage(msg);
        }

        public void NotifyConfigurationUpdate(ConfigUpdateResult result, Guid replyToId)
        {
            var msg = new NotifyConfigUpdateMessage(result);
            msg.ReplyToId = replyToId;
            PushMessage(msg);
        }

        /// <summary>
        /// Send captive portal state back to the client.
        /// </summary>
        /// <param name="captivePortalState"></param>
        public void SendCaptivePortalState(bool captivePortalState, bool isCaptivePortalActive)
        {
            var msg = new CaptivePortalDetectionMessage(captivePortalState, isCaptivePortalActive);
            PushMessage(msg);
        }

        protected abstract void PushMessage(BaseMessage msg);

        public abstract void Dispose();
    }
}