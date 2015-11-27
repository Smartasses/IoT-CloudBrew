using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FakeDevice
{
    public class DeviceId
    {
        public static Guid? Current
        {
            get
            {
                string id = null;
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("deviceconfig"))
                {
                    id = (string) ApplicationData.Current.LocalSettings.Values["deviceconfig"];
                }

                if (!string.IsNullOrEmpty(id))
                {
                    return new Guid(id);
                }

                return null;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values["deviceconfig"] = value.HasValue ? value.Value.ToString() : "";
            }
        }
    }

}
