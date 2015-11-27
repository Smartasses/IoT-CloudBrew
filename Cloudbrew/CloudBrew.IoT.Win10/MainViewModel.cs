using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.UI.Core;
using CloudBrew.IoT.Win10.Annotations;

namespace CloudBrew.IoT.Win10
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly CoreDispatcher _dispatcher;
        private string _deviceId;
        private SerialCommunication _serialCommunication;
        private StringBuilder _sbLog;

        public MainViewModel(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            DeviceId = "**Device id**";
            Init();
            _sbLog = new StringBuilder();
            WriteLog("Registered!");
        }

        private async void Init()
        {
            var deviceRegistration = new DeviceRegistration();
            var device = await deviceRegistration.GetOrCreateDevice();
            DeviceId = device.Id.ToString();
            var serialPorts = await SerialCommunication.ListAvailablePorts();
            _serialCommunication = new SerialCommunication(serialPorts.First());
            _serialCommunication.OnMessage += OnSerialMessage;
        }

        private void OnSerialMessage(object sender, MessageEventArgs e)
        {
            WriteLog(e.Message);
            WriteLog("Sending message: 1000");
            _serialCommunication.SendAsync("1000");
        }

        public string DeviceId
        {
            get { return _deviceId; }
            set
            {
                if (value == _deviceId) return;
                _deviceId = value;
                OnPropertyChanged();
            }
        }

        public string Log => _sbLog.ToString();

        public void WriteLog(string message)
        {
            _sbLog.AppendLine(message);
            OnPropertyChanged("Log");
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (_dispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            else
            {
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged(propertyName));
            }
        }
    }
}