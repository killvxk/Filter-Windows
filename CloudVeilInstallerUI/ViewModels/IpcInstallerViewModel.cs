using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudVeilInstallerUI.IPC;
using CloudVeilInstallerUI.Models;
using NamedPipeWrapper;

namespace CloudVeilInstallerUI.ViewModels
{
    public class RemoteInstallerViewModel : IInstallerViewModel
    {

        UpdateIPCClient client;

        public RemoteInstallerViewModel(UpdateIPCClient client)
        {
            this.client = client;

            client.MessageReceived += CheckPropertyChangedMessage;
        }

        private void CheckPropertyChangedMessage(NamedPipeConnection<Message, Message> connection, Message message)
        {
            if(message.Command == Command.PropertyChanged)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(message.Property));
            }
        }

        private void set<TSettable>(string prop, TSettable val)
        {
            client.Set("InstallerViewModel", prop, val);
        }

        private void call(string method, params object[] parameters)
        {
            client.Call("InstallerViewModel", method, parameters);
        }

        private string welcomeButtonText;
        public string WelcomeButtonText
        {
            get => welcomeButtonText;
            set
            {
                welcomeButtonText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeButtonText)));
            }
        }

        private string welcomeHeader;
        public string WelcomeHeader { get => welcomeHeader; set { welcomeHeader = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeHeader))); } }

        private string welcomeText;
        public string WelcomeText { get => welcomeText; set { welcomeText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeText))); } }

        private bool showPrompts;
        public bool ShowPrompts { get => showPrompts; set { showPrompts = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowPrompts))); } }

        private InstallType installType;
        public InstallType InstallType { get => installType; set { installType = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstallType))); } }

        private InstallationState state;
        public InstallationState State { get => state; set { state = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State))); } }

        private string description;
        public string Description { get => description; set { description = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description))); } }

        private bool isIndeterminate;
        public bool IsIndeterminate { get => isIndeterminate; set { isIndeterminate = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIndeterminate))); } }

        private int progress;
        public int Progress { get => progress; set { progress = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress))); } }

        private string finishedHeading;
        public string FinishedHeading { get => finishedHeading; set { finishedHeading = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinishedHeading))); } }

        private string finishedMessage;
        public string FinishedMessage { get => finishedMessage; set { finishedMessage = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinishedMessage))); } }

        private string finishButtonText;
        public string FinishButtonText { get => finishButtonText; set { finishButtonText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinishButtonText))); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void TriggerDetect() => Task.Run(() => call("TriggerDetect"));
        public void TriggerFailed(string message, string heading = null) => Task.Run(() => call("TriggerFailed", message, heading));
        public void TriggerFinished() => Task.Run(() => call("TriggerFinished"));
        public void TriggerInstall() => Task.Run(() => call("TriggerInstall"));
        public void TriggerWelcome() => Task.Run(() => call("TriggerWelcome"));
        public void Exit() => call("Exit");
    }
}
