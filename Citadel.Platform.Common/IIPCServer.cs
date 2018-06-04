using System;
using Citadel.IPC.Messages;
using Citadel.Platform.Common.Types;

namespace Citadel.Platform.Common.IPC
{
    public interface IIPCServer
    {
        /// <summary>
        /// Delegate to be called when a client requests a relaxed policy. 
        /// </summary>
        RelaxPolicyRequestHander RelaxedPolicyRequested { get; set; }

        /// <summary>
        /// Delegate to be called when a client requests filter deactivation. 
        /// </summary>
        DeactivationRequestHandler DeactivationRequested { get; set; }

        /// <summary>
        /// Delegate to be called when a client attempts to authenticate. 
        /// </summary>
        AuthenticationRequestHandler AttemptAuthentication { get; set; }

        /// <summary>
        /// Delegate to be called when a client connects. 
        /// </summary>
        ServerGenericParameterlessHandler ClientConnected { get; set; }

        /// <summary>
        /// Delegate to be called when a client disconnects. 
        /// </summary>
        ServerGenericParameterlessHandler ClientDisconnected { get; set; }

        /// <summary>
        /// Delegate to be called when a client is querying the state of the filter. 
        /// </summary>
        StateChangeHandler ClientServerStateQueried { get; set; }

        /// <summary>
        /// Delegate to be called when a client has responded accepting a pending
        /// application update.
        /// </summary>
        ServerGenericParameterlessHandler ClientAcceptedPendingUpdate { get; set; }

        /// <summary>
        /// Delegate to be called when a client has submitted a block action for
        /// review.
        /// </summary>
        BlockActionReportHandler ClientRequestsBlockActionReview { get; set; }

        /// <summary>
        /// Delegate to be called when a client is requesting a configuration/ruleset update.
        /// </summary>
        RequestConfigUpdateHandler RequestConfigUpdate { get; set; }

        /// <summary>
        /// Delegate to be called when a client is requesting a captive portal state.
        /// </summary>
        RequestCaptivePortalDetectionHandler RequestCaptivePortalDetection { get; set; }

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
        void NotifyRelaxedPolicyChange(int numPoliciesAvailable, TimeSpan policyDuration, RelaxedPolicyStatus status)
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
        void NotifyStatus(FilterStatus status);

        void NotifyCooldownEnforced(TimeSpan cooldownPeriod);

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
        void NotifyBlockAction(BlockType type, Uri resource, string category, string rule = null);

        /// <summary>
        /// Notifies clients of the current authentication state. 
        /// </summary>
        /// <param name="action">
        /// The authentication command which reflects the current auth state. 
        /// </param>
        void NotifyAuthenticationStatus(AuthenticationAction action, AuthenticationResultObject authenticationResult = null);

        /// <summary>
        /// Notifies all clients of an available update. 
        /// </summary>
        /// <param name="message">
        /// The update message.
        /// </param>
        void NotifyApplicationUpdateAvailable(ServerUpdateQueryMessage message);

        /// <summary>
        /// Notifies all clients that the server is updating itself.
        /// </summary>
        void NotifyUpdating();

        void NotifyConfigurationUpdate(ConfigUpdateResult result, Guid replyToId);

        /// <summary>
        /// Send captive portal state back to the client.
        /// </summary>
        /// <param name="captivePortalState"></param>
        void SendCaptivePortalState(bool captivePortalState, bool isCaptivePortalActive);
    }
}
