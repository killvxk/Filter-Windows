using AppKit;
using Citadel.IPC.Messages;
using Citadel.Platform.Common.IPC;
using Filter.Platform.Mac.IPC;
using Foundation;

namespace CloudVeil
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        IPCClient ipcClient;

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Insert code here to initialize your application
            ipcClient = new MacIPCClient();

            ipcClient.ConnectedToServer = () => System.Console.WriteLine("Connected to server.");

            ipcClient.AuthenticationResultReceived = HandleAuthenticationResult;
            ipcClient.RelaxedPolicyInfoReceived = HandleRelaxedPolicyInfoReceived;
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        void HandleAuthenticationResult(AuthenticationMessage result)
        {
            System.Console.WriteLine($"Authentication result received: {result.Action} {result.AuthenticationResult.AuthenticationResult}");
        }


        void HandleRelaxedPolicyInfoReceived(RelaxedPolicyMessage msg)
        {
            System.Console.WriteLine($"Relaxed policy info received: {msg.Command} {msg.PolicyInfo.NumberAvailableToday}");
        }

    }
}
