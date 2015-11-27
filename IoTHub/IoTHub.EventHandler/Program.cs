using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
    public class Program
    {
        private static EventHubClient _eventHubClient;
        private static ServiceClient _serviceClient;
        private static JobHost host;
        static void Main()
        {
            host = new JobHost();
            var connectionString = ConfigurationManager.ConnectionStrings["IoTHub"].ConnectionString;
            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, "messages/events");
            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            Task.WaitAll(_eventHubClient.GetRuntimeInformation().PartitionIds.Select(Listen).ToArray());
            host.RunAndBlock();
        }

        static async Task Listen(string partitionId)
        {
            Console.WriteLine("Listening to partition {0}" + partitionId);
            var receiver = await _eventHubClient.GetDefaultConsumerGroup().CreateReceiverAsync(partitionId, DateTime.UtcNow);
            while (true)
            {
                var eventData = await receiver.ReceiveAsync();
                if (eventData == null) continue;
                var message = Encoding.ASCII.GetString(eventData.GetBytes());
                var deviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
                Console.WriteLine("Message received: {0}", message);
                await host.CallAsync(typeof(Program).GetMethod("HandleButtonEvent"), new { deviceId, message });
            }
        }

        [NoAutomaticTrigger]
        public static async Task HandleButtonEvent(Guid deviceId, string message, TextWriter log)
        {
            var response = "1000";
            var sendCommand = new Message(Encoding.ASCII.GetBytes(response));
            await _serviceClient.SendAsync(deviceId.ToString(), sendCommand);
            log.WriteLine("Message send: {0}", response);
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
