using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FakeDevice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DeviceClient _client;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async Task ChangeLightStatus(bool lightOn)
        {
            if (!this.Dispatcher.HasThreadAccess)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await ChangeLightStatus(lightOn));
                return;
            }
            if (lightOn)
            {
                Ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));

            }
            else
            {
            Ellipse.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
        }

        private async void Button_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            await SendEvent(new IoTButtonChangedMessage
            {
                ButtonPressed = true
            });
        }

        private async void Button_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            await SendEvent(new IoTButtonChangedMessage
            {
                ButtonPressed = false
            });
        }

        private async Task SendEvent(IoTButtonChangedMessage e)
        {
            var seralized = JsonConvert.SerializeObject(e);
            var sendCommand = new Message(Encoding.ASCII.GetBytes(seralized));
            await _client.SendEventAsync(sendCommand);
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            _client = DeviceClient.Create("cloudbrew.azure-devices.net", new DeviceAuthenticationWithRegistrySymmetricKey("ae048b8a-37b0-4a1a-b498-0c5e6bac1876", "8ocFvhlTXlt3gF01kUfRvJaSPE1c6zi4P8/4TjcPS1g="), TransportType.Http1); ;
            this.button.AddHandler(PointerPressedEvent, new PointerEventHandler(Button_OnPointerPressed), true);
            this.button.AddHandler(PointerReleasedEvent, new PointerEventHandler(Button_OnPointerReleased), true);

            Task.Run(async () =>
            {
                while (true)
                {
                    var message = await _client.ReceiveAsync();
                    if (message == null) continue;
                    var messageContent = Encoding.ASCII.GetString(message.GetBytes());
                    var result = JsonConvert.DeserializeObject<IoTChangeLightMessage>(messageContent);
                    await ChangeLightStatus(result.LightOn);
                    await _client.CompleteAsync(message);
                }
            });
        }
    }

    class IoTButtonChangedMessage
    {
        public bool ButtonPressed { get; set; }
    }
    class IoTChangeLightMessage
    {
        public bool LightOn { get; set; }
    }
}
