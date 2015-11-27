using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;

namespace CloudBrew.IoT.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        private static EventHubClient _eventHubClient;
        private static ServiceClient _serviceClient;

        private static JobHost _host;
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            _host = new JobHost();
            var connectionString = ConfigurationManager.ConnectionStrings["IoTHub"].ConnectionString;
            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, "messages/events");
            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            Task.WaitAll(_eventHubClient.GetRuntimeInformation().PartitionIds.Select(Listen).ToArray());
            _host.RunAndBlock();
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

                await _host.CallAsync(typeof(Program).GetMethod("SendMessageBackToClient"), new { deviceId, message = "1000" });
            }
        }

        [NoAutomaticTrigger]
        public static async Task SendMessageBackToClient(Guid deviceId, string message, TextWriter log)
        {
            var sendCommand = new Message(Encoding.ASCII.GetBytes(message));
            await _serviceClient.SendAsync(deviceId.ToString(), sendCommand);
            log.WriteLine("Message send: {0}", message);
        }
    }
}
