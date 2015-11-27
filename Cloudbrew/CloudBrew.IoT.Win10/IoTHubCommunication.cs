using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace CloudBrew.IoT.Win10
{
    class IoTHubCommunication
    {
        private DeviceClient _client;

        public IoTHubCommunication(Device device)
        {
            _client = DeviceClient.Create("cloudbrew.azure-devices.net", 
                new DeviceAuthenticationWithRegistrySymmetricKey(device.Id.ToString(), device.Key),
                TransportType.Http1);

            Task.Run(async () =>
            {
                while (true)
                {
                    var message = await _client.ReceiveAsync();
                    if (message == null) continue;
                    var messageContent = Encoding.ASCII.GetString(message.GetBytes());
                    InvokeOnMessage(new MessageEventArgs {Message = messageContent});
                    await _client.CompleteAsync(message);
                }
            });
        }
        public event EventHandler<MessageEventArgs> OnMessage;

        public async Task SendAsync(string message)
        {
            var sendCommand = new Message(Encoding.ASCII.GetBytes(message));
            await _client.SendEventAsync(sendCommand);
        }

        protected virtual void InvokeOnMessage(MessageEventArgs e)
        {
            OnMessage?.Invoke(this, e);
        }
    }
}
