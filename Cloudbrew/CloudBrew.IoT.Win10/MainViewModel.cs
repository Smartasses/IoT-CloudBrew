using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using CloudBrew.IoT.Win10.Annotations;
using Microsoft.Azure.Devices.Client;

namespace CloudBrew.IoT.Win10
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly CoreDispatcher _dispatcher;
        private string _deviceId;
        private SerialCommunication _serialCommunication;
        private StringBuilder _sbLog;
        private IoTHubCommunication _iotHubCommunication;
        private bool _ledOn;

        public MainViewModel(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            DeviceId = "**Device id**";
            Init();
            _sbLog = new StringBuilder();
        }

        private async void Init()
        {
            var deviceRegistration = new DeviceRegistration();
            var device = await deviceRegistration.GetOrCreateDevice();
            DeviceId = device.Id.ToString();
            var serialPorts = await SerialCommunication.ListAvailablePorts();
            _serialCommunication = new SerialCommunication(serialPorts.First());
            _serialCommunication.OnMessage += OnSerialMessage;
            _iotHubCommunication = new IoTHubCommunication(device);
            _iotHubCommunication.OnMessage += OnHubMessage;
        }

        private void OnHubMessage(object sender, MessageEventArgs e)
        {
            WriteLog("Received message from the hub: " + e.Message);
            WriteLog("Sending serial message: " + e.Message);
            _serialCommunication.SendAsync(e.Message);
            Task.Run(async () =>
            {
                ChangeLedState(true);
                await Task.Delay(int.Parse(e.Message));
                ChangeLedState(false);
            });
        }
        public SolidColorBrush LedColor
        {
            get { return _ledOn ? new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)) : new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)); }
        }

        public void ChangeLedState(bool ledState)
        {
            _ledOn = ledState;
            OnPropertyChanged(nameof(LedColor));
        }

        private void OnSerialMessage(object sender, MessageEventArgs e)
        {
            WriteLog(e.Message);
            WriteLog("Sending message: to the iot hub");
            _iotHubCommunication.SendAsync(e.Message);
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