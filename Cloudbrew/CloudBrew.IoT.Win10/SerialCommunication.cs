using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace CloudBrew.IoT.Win10
{

    class SerialCommunication : IDisposable
    {
        private readonly DeviceInformation _device;
        private SerialDevice _serialPort;
        private DataReader _dataReader;
        private readonly CancellationTokenSource _cancellation;

        public SerialCommunication(DeviceInformation device)
        {
            _device = device;
            _cancellation = new CancellationTokenSource();
            Init();
        }

        private async Task Init()
        {
            _serialPort = await SerialDevice.FromIdAsync(_device.Id);

            // Configure serial settings
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = SerialParity.None;
            _serialPort.StopBits = SerialStopBitCount.One;
            _serialPort.DataBits = 8;
            _dataReader = new DataReader(_serialPort.InputStream);
            await Task.Run(async () =>
            {
                while (!_cancellation.Token.IsCancellationRequested)
                {
                    await ReadAsync();
                }
            }, _cancellation.Token);
        }
        private async Task ReadAsync()
        {
            _cancellation.Token.ThrowIfCancellationRequested();
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            var loadAsyncTask = _dataReader.LoadAsync(20).AsTask(_cancellation.Token);
            var bytesRead = await loadAsyncTask;
            if (bytesRead > 0)
            {
                var readString = _dataReader.ReadString(bytesRead);
                InvokeOnMessage(readString);
            }
        }
        public async Task SendAsync(string text)
        {
            var dataWriteObject = new DataWriter(_serialPort.OutputStream);
            dataWriteObject.WriteString(text);
            var task = dataWriteObject.StoreAsync().AsTask(_cancellation.Token);
            await task;
            dataWriteObject.DetachStream();
        }

        public static async Task<IEnumerable<DeviceInformation>> ListAvailablePorts()
        {
            var aqs = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            return dis.ToArray();
        }

        public event EventHandler<MessageEventArgs> OnMessage;

        public void Dispose()
        {
            _cancellation.Cancel();
            _dataReader.Dispose();
            _serialPort.Dispose();
        }

        protected virtual void InvokeOnMessage(string message)
        {
            OnMessage?.Invoke(this, new MessageEventArgs { Message = message });
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }
}
