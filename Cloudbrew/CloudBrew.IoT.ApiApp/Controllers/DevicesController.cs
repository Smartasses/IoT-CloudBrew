using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Devices;

namespace CloudBrew.IoT.ApiApp.Controllers
{

    public class DevicesController : ApiController
    {
        private readonly RegistryManager _manager;

        public DevicesController()
        {
            _manager = RegistryManager.CreateFromConnectionString(ConfigurationManager.ConnectionStrings["IoTHub"].ConnectionString);
        }

        public async Task<IEnumerable<Device>> GetAll()
        {
            var devices = await _manager.GetDevicesAsync(1000);
            return devices.Select(x => new Device(x));
        }

        public async Task<Device> Post()
        {
            var device = await _manager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(Guid.NewGuid().ToString()));
            return new Device(device);
        }

        public async Task<Device> Get(Guid id)
        {
            var device = await _manager.GetDeviceAsync(id.ToString());
            return new Device(device);
        }

        public async Task Delete(Guid id)
        {
            await _manager.RemoveDeviceAsync(id.ToString());
        }
    }

    public class Device
    {
        public Device(Microsoft.Azure.Devices.Device device)
        {
            Id = new Guid(device.Id);
            Key = device.Authentication.SymmetricKey.PrimaryKey;
        }

        public Guid Id { get; set; }
        public string Key { get; set; }
    }
}
