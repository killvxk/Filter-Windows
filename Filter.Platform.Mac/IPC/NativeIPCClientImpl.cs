using System;
using System.Runtime.InteropServices;

namespace Filter.Platform.Mac.IPC
{
    public delegate void ConnectionCallbackDelegate();

    public static class NativeIPCClientImpl
    {

        [DllImport("libFilterLibs.Platform.Mac.dylib")]
        public static extern IntPtr CreateIPCClient(MessageCallbackDelegate callbackDelegate, ConnectionCallbackDelegate onConnect);

        [DllImport("libFilterLibs.Platform.Mac.dylib", EntryPoint = "IPCClient_Connect")]
        public static extern void Connect(IntPtr handle, string serverName);

        [DllImport("libFilterLibs.Platform.Mac.dylib", EntryPoint = "IPCClient_Disconnect")]
        public static extern void Disconnect(IntPtr handle);

        [DllImport("libFilterLibs.Platform.Mac.dylib", EntryPoint = "IPCClient_Send")]
        public static extern void Send(IntPtr handle, byte[] data, int dataLength);

        [DllImport("libFilterLibs.Platform.Mac.dylib", EntryPoint = "IPCClient_SetCallback")]
        public static extern void SetCallback(IntPtr handle, MessageCallbackDelegate callbackDelegate);

        [DllImport("libFilterLibs.Platform.Mac.dylib", EntryPoint = "IPCClient_Release")]
        public static extern void Release(IntPtr handle);

        [DllImport("libFilterLibs.Platform.Mac.dylib", EntryPoint = "IPCClient_IsConnected")]
        public static extern bool IsConnected(IntPtr handle);
    }
}
