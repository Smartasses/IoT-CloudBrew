using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace CloudBrew.IoT.Win10
{
    class DeviceRegistration
    {
        private readonly HttpClient _httpClient;

        public DeviceRegistration()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://microsoft-apiapp171aef7e7d9f489fa2a6e4659d5e2322.azurewebsites.net/")
            };
        }

        public async Task<Device> GetOrCreateDevice()
        {
            object setting;
            Guid id;
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue("deviceconfig", out setting)
                && Guid.TryParse(setting.ToString(), out id))
            {
                var response = await _httpClient.GetAsync("api/devices/" + id);
                if (response.IsSuccessStatusCode)
                {
                    var contentText = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Device>(contentText);
                }
                else if(response.StatusCode != HttpStatusCode.NotFound)
                {
                    response.EnsureSuccessStatusCode();
                }
            }
            // Device doesn't exist anymore or isn't registered
            {
                var response = await _httpClient.PostAsync("api/devices", new StringContent(""));
                response.EnsureSuccessStatusCode();
                var contentText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Device>(contentText);
            }
        }
    }


    public class Device
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
    }
}
