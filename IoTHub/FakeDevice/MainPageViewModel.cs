using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace FakeDevice
{
    class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly CoreDispatcher _dispatcher;

        public async Task Init()
        {
            try
            {
                var device = await EnsureDeviceIsRegistered();
                await WriteLog("Registered device: " + device.Id + ", key: " + device.Key);
                _iotHubClient = DeviceClient.Create("cloudbrew.azure-devices.net",
                    new DeviceAuthenticationWithRegistrySymmetricKey(device.Id.ToString(), device.Key),
                    TransportType.Http1);

                SerialPorts = await SerialCommunication.ListAvailablePorts();
                SerialPort = SerialPorts.First();

                Task.Run(async () =>
                {
                    while (true)
                    {
                        var message = await _iotHubClient.ReceiveAsync();
                        if (message == null) continue;
                        var messageContent = Encoding.ASCII.GetString(message.GetBytes());
                        WriteLog("IoT Hub - RECEIVE: " + messageContent);
                        if (_serial != null)
                        {
                            WriteLog("Serial - SEND: " + messageContent);
                            await _serial.SendAsync(messageContent);
                            LedOn = true;
                            Task.Run(async () =>
                            {
                                await Task.Delay(int.Parse(messageContent));
                                LedOn = false;
                            });
                        }
                        await _iotHubClient.CompleteAsync(message);
                    }
                });
            }
            catch (Exception e)
            {
                await WriteLog(e.ToString());
            }
        }

        public bool LedOn
        {
            get { return _ledOn; }
            set
            {
                _ledOn = value;
                _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged("LedColor"));
            }
        }

        public SolidColorBrush LedColor
        {
            get { return LedOn ? new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)) : new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)); }
        }

        private static async Task<Device> EnsureDeviceIsRegistered()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://microsoft-apiapp171aef7e7d9f489fa2a6e4659d5e2322.azurewebsites.net/api/Devices")
            };
            Device device = null;
            if (DeviceId.Current.HasValue)
            {
                var response = await client.GetAsync("/" + DeviceId.Current.Value);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    device = JsonConvert.DeserializeObject<Device>(content);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    DeviceId.Current = null;
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            if (device == null)
            {
                var response = await client.PostAsync("", new StringContent(""));
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    device = JsonConvert.DeserializeObject<Device>(content);
                    DeviceId.Current = device.Id;
                }
                else
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            return device;
        }

        public class Device
        {
            public Guid Id { get; set; }
            public string Key { get; set; }
        }
        private DeviceInformation _port;
        private SerialCommunication _serial;
        private DeviceClient _iotHubClient;
        private string _log;
        private SolidColorBrush _ledColor;
        private bool _ledOn;

        public MainPageViewModel(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            Log = "";
        }

        public string Log
        {
            get { return _log; }
            set
            {
                if (value == _log) return;
                _log = value;
                OnPropertyChanged("Log");
            }
        }

        public async Task WriteLog(string text)
        {
            Debug.WriteLine("{0:HH:mm:ss.ffff} - {1}", DateTime.Now, text);
        }
        public DeviceInformation SerialPort
        {
            get { return _port; }
            set
            {
                _port = value;
                if (_serial != null)
                {
                    _serial.OnMessage -= ReceiveSerialMessage;
                    _serial.Dispose();
                }
                _serial = new SerialCommunication(value);
                _serial.OnMessage += ReceiveSerialMessage;
            }
        }

        private async void ReceiveSerialMessage(object sender, MessageEventArgs e)
        {
            await WriteLog("Serial - RECEIVE: " + e.Message);
            var sendCommand = new Message(Encoding.ASCII.GetBytes(e.Message));
            await _iotHubClient.SendEventAsync(sendCommand);
            await WriteLog("IoT Hub - SEND: " + e.Message);
        }

        public IEnumerable<DeviceInformation> SerialPorts { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}