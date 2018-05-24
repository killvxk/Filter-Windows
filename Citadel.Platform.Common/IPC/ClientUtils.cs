using System;
namespace Citadel.Platform.Common.IPC
{
    public class StateChangeEventArgs : EventArgs
    {
        public FilterStatus State
        {
            get;
            private set;
        }

        public TimeSpan CooldownPeriod
        {
            get;
            private set;
        } = TimeSpan.Zero;

        public StateChangeEventArgs(FilterStatusMessage msg)
        {
            State = msg.Status;
            CooldownPeriod = msg.CooldownDuration;
        }
    }

    public delegate void StateChangeHandler(StateChangeEventArgs args);

    public delegate void AuthenticationResultHandler(AuthenticationMessage result);

    public delegate void DeactivationRequestResultHandler(DeactivationCommand args);

    public delegate AuthenticationMessage GetAuthMessage();

    public delegate void BlockActionReportHandler(NotifyBlockActionMessage msg);

    public delegate void RelaxedPolicyInfoReceivedHandler(RelaxedPolicyMessage msg);

    public delegate void ClientGenericParameterlessHandler();

    public delegate void ServerUpdateRequestHandler(ServerUpdateQueryMessage msg);

    public delegate void CaptivePortalDetectionHandler(CaptivePortalDetectionMessage msg);

    /// <summary>
    /// A generic reply handler, called by IPC queue.
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    public delegate bool GenericReplyHandler(BaseMessage msg);
}
