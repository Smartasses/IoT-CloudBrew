using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace IoTHub.EventHandler
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        private static EventHubClient _eventHubClient;
        private static ServiceClient _serviceClient;
        private static JobHost host;
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            host = new JobHost();
            // The following code ensures that the WebJob will be running continuously

            var connectionString = ConfigurationManager.ConnectionStrings["IoTHub"].ConnectionString;
            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, "messages/events");
            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            var tasks = new List<Task>();
            foreach (var partitionId in _eventHubClient.GetRuntimeInformation().PartitionIds)
            {
                tasks.Add(Listen(partitionId));
            }
            Task.WaitAll(tasks.ToArray());
            host.RunAndBlock();
        }

        static async Task Listen(string partitionId)
        {
            Console.WriteLine("Listening to partition {0}" + partitionId);
            var receiver = await _eventHubClient.GetDefaultConsumerGroup().CreateReceiverAsync(partitionId, DateTime.UtcNow);
            while (true)
            {
                var eventData = await receiver.ReceiveAsync();
                var message = Encoding.ASCII.GetString(eventData.GetBytes());
                var result = JsonConvert.DeserializeObject<IoTButtonChangedMessage>(message);
                var deviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
                var buttonState = result.ButtonPressed;
                Console.WriteLine("Message received: {0}", message);
                await host.CallAsync(typeof(Program).GetMethod("HandleButtonEvent"), new { deviceId, buttonState });
            }
        }

        [NoAutomaticTrigger]
        public static async Task HandleButtonEvent(Guid deviceId, bool buttonState)
        {
            var seralized = JsonConvert.SerializeObject(new IoTChangeLightMessage { LightOn = buttonState });
            var sendCommand = new Message(Encoding.ASCII.GetBytes(seralized));
            await _serviceClient.SendAsync(deviceId.ToString(), sendCommand);
            Console.WriteLine("Message send: {0}", seralized);
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
