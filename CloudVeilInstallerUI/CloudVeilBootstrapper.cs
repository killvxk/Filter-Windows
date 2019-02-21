using CloudVeilInstallerUI.IPC;
using CloudVeilInstallerUI.ViewModels;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using NamedPipeWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CloudVeilInstallerUI
{
    public interface IBootstrapper
    {

    }

    public class LocalBootstrapperImpl : IBootstrapper
    {
        CloudVeilBootstrapper ba;

        public LocalBootstrapperImpl(CloudVeilBootstrapper ba)
        {
            this.ba = ba;
        }
    }

    public class IpcBootstrapperImpl : IBootstrapper
    {
        CloudVeilBootstrapper ba;

        public IpcBootstrapperImpl(CloudVeilBootstrapper ba)
        {
            this.ba = ba;
        }
    }

    public class CloudVeilBootstrapper : BootstrapperApplication
    {
        public static Dispatcher BootstrapperDispatcher { get; private set; }

        private UpdateIPCServer server = null;

        protected override void Run()
        {
            string[] args = this.Command.GetCommandLineArgs();

            bool runIpc = false;
            bool showPrompts = true;

            foreach(string arg in args)
            {
                if(arg == "/ipc")
                {
                    runIpc = true;
                }
                else if(arg == "/nomodals")
                {
                    showPrompts = false;
                }
            }

            BootstrapperDispatcher = Dispatcher.CurrentDispatcher;

            Application app = new Application();

            ISetupUI setupUi = null;
            InstallerViewModel model = new InstallerViewModel(this);

            if (runIpc)
            {
                server = new UpdateIPCServer("__CloudVeilUpdaterPipe__");
                
                server.MessageReceived += CheckExit;
                server.ClientConnected += clientConnected;
                server.ClientDisconnected += clientDisconnected;

                server.RegisterObject("InstallerViewModel", model);
                server.RegisterObject("SetupUI", setupUi);
                server.Start();

                BootstrapperDispatcher.ShutdownStarted += (sender, e) =>
                {
                    server.PushMessage(new Message()
                    {
                        Command = IPC.Command.Exit
                    });
                };

                server.MessageReceived += CheckStartCommand; // Wait for the first start command to begin installing.

                setupUi = new IpcWindow(server, model, showPrompts);
                setupUi.Closed += (sender, e) => BootstrapperDispatcher.InvokeShutdown();

                model.SetSetupUi(setupUi);

                model.PropertyChanged += (sender, e) =>
                {
                    Type t = model.GetType();
                    var prop = t.GetProperty(e.PropertyName);

                    server.Set("InstallerViewModel", e.PropertyName, prop.GetValue(model));
                };

                this.Engine.Detect();
            }
            else
            {
                setupUi = new MainWindow(model, showPrompts);
                setupUi.Closed += (sender, e) => BootstrapperDispatcher.InvokeShutdown();

                model.SetSetupUi(setupUi);
                this.Engine.Detect();

                setupUi.Show();
                Dispatcher.Run();
                this.Engine.Log(LogLevel.Standard, "Dispatcher Run has completed");
                this.Engine.Quit(0);
            }
        }

        private void CheckStartCommand(NamedPipeConnection<Message, Message> connection, Message message)
        {
            if(message.Command == IPC.Command.Start)
            {
                Thread t = new Thread(() =>
                {
                    Dispatcher.Run();
                    Engine.Quit(0);
                });

                t.Start();
            }
        }

        private void CheckExit(NamedPipeConnection<Message, Message> connection, Message message)
        {
            if(message.Command == IPC.Command.Exit)
            {
                BootstrapperDispatcher.InvokeShutdown();
            }
        }

        private int numClients = 0;

        private void clientConnected(object sender, EventArgs e)
        {
            numClients++;
            Engine.Log(LogLevel.Standard, $"NumClients = {numClients}");

            var objs = server.VariableObjects;

            // Whenever a new client (CloudVeilUpdater) connects, do an initial state synchronization.
            foreach(KeyValuePair<string, object> kvp in objs)
            {
                object o = kvp.Value;

                Type t = o.GetType();
                PropertyInfo[] props = t.GetProperties();

                foreach(var prop in props)
                {
                    server.Set(kvp.Key, prop.Name, prop.GetValue(o));
                }
            }
        }

        private void clientDisconnected(object sender, EventArgs e)
        {
            numClients--;
            Engine.Log(LogLevel.Standard, $"NumClients = {numClients}");

            if(numClients <= 0)
            {
                BootstrapperDispatcher.InvokeShutdown();
            }
        }
    }
}
