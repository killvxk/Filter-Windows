using System;
using System.Security;
using Citadel.IPC.Messages;

namespace Citadel.Platform.Common.IPC
{
    /// <summary>
    /// Arguments for the RelaxPolicyRequestHander delegate. 
    /// </summary>
    public class RelaxedPolicyEventArgs : EventArgs
    {
        /// <summary>
        /// The relaxed policy command issued by the client. 
        /// </summary>
        public RelaxedPolicyCommand Command
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructs a new RelaxedPolicyEventArgs from the given client message. 
        /// </summary>
        /// <param name="msg">
        /// The client message. 
        /// </param>
        public RelaxedPolicyEventArgs(RelaxedPolicyMessage msg)
        {
            Command = msg.Command;
        }
    }

    /// <summary>
    /// Delegate for the handler of client relaxed policy messages. 
    /// </summary>
    /// <param name="args">
    /// Relaxed policy request arguments. 
    /// </param>
    public delegate void RelaxPolicyRequestHander(RelaxedPolicyEventArgs args);

    public enum RequestState
    {
        NoResponse = 0,
        Granted,
        Denied
    }

    /// <summary>
    /// Arguments for the DeactivationRequestHandler delegate. 
    /// </summary>
    public class DeactivationRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Whether or not the request was granted. Defaults to false. 
        /// </summary>
        public bool Granted
        {
            get
            {
                return DeactivationCommand == DeactivationCommand.Granted;
            }
        }



        /// <summary>
        /// Did we successfully send the request and get any response back?
        /// </summary>
        public DeactivationCommand DeactivationCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Constructs a new DeactivationRequestEventArgs instance. 
        /// </summary>
        public DeactivationRequestEventArgs()
        {
        }
    }

    /// <summary>
    /// Delegate for the handler of client deactivation requests. 
    /// </summary>
    /// <param name="args">
    /// Deactivation request arguments. 
    /// </param>
    public delegate void DeactivationRequestHandler(DeactivationRequestEventArgs args);

    /// <summary>
    /// Arguments for the AuthenticationRequestHandler delegate. 
    /// </summary>
    public class AuthenticationRequestArgs : EventArgs
    {
        /// <summary>
        /// The username with which to attempt authentication. 
        /// </summary>
        public string Username
        {
            get;
            private set;
        }

        /// <summary>
        /// The password with which to attempt authentication. 
        /// </summary>
        public SecureString Password
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructs a new AuthenticationRequestArgs instance from the given client message. 
        /// </summary>
        /// <param name="msg">
        /// The client authentication message. 
        /// </param>
        public AuthenticationRequestArgs(AuthenticationMessage msg)
        {
            Username = msg.Username;
            Password = new SecureString();

            try
            {
                foreach (var c in msg.Password)
                {
                    Password.AppendChar((char)c);
                }
            }
            finally
            {
                Array.Clear(msg.Password, 00, msg.Password.Length);
            }
        }
    }

    /// <summary>
    /// Delegate for the handler of client authentication requests. 
    /// </summary>
    /// <param name="args">
    /// Authentication request arguments. 
    /// </param>
    public delegate void AuthenticationRequestHandler(AuthenticationRequestArgs args);

    /// <summary>
    /// Generic void, parameterless delegate.
    /// </summary>
    public delegate void ServerGenericParameterlessHandler();

    public delegate void RequestConfigUpdateHandler(RequestConfigUpdateMessage message);

    /// <summary>
    /// Handler for requesting a captive portal detection.
    /// </summary>
    /// <param name="message"></param>
    public delegate void RequestCaptivePortalDetectionHandler(CaptivePortalDetectionMessage message);

}
