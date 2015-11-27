using System;
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
using Windows.Devices.SerialCommunication;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FakeDevice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }


        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                var vm = new MainPageViewModel(Dispatcher);
                await vm.Init();
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    DataContext = vm;
                });
            });
        }
    }
}
