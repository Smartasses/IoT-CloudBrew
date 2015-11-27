using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using CloudBrew.IoT.Win10.Annotations;

namespace CloudBrew.IoT.Win10
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly CoreDispatcher _dispatcher;
        private string _deviceId;

        public MainViewModel(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            DeviceId = "**Device id**";
            Init();
        }

        private async void Init()
        {
            var deviceRegistration = new DeviceRegistration();
            var device = await deviceRegistration.GetOrCreateDevice();
            DeviceId = device.Id.ToString();
        }

        public string DeviceId
        {
            get { return _deviceId; }
            set
            {
                if (_dispatcher.HasThreadAccess)
                {
                    if (value == _deviceId) return;
                    _deviceId = value;
                    OnPropertyChanged();
                }
                else
                {
                    _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DeviceId = value);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}