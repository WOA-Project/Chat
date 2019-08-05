using Chat.Controls;
using Chat.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Sms;

namespace Chat.ViewModels
{
    public class ComposeViewModel : Observable
    {
        private ObservableCollection<CellularLineControl> _cellularLines;
        public ObservableCollection<CellularLineControl> CellularLines
        {
            get { return _cellularLines; }
            set { Set(ref _cellularLines, value); }
        }

        // Constructor
        public ComposeViewModel()
        {
            Initialize();
        }


        // Initialize Stuff
        public async void Initialize()
        {
            CellularLines = await GetSmsDevices();
        }

        // Methods
        private async Task<ObservableCollection<CellularLineControl>> GetSmsDevices()
        {
            ObservableCollection<CellularLineControl> collection = new ObservableCollection<CellularLineControl>();
            var smsDevices = await DeviceInformation.FindAllAsync(SmsDevice2.GetDeviceSelector(), null);
            foreach (var smsDevice in smsDevices)
            {
                try
                {
                    SmsDevice2 dev = SmsDevice2.FromId(smsDevice.Id);
                    CellularLineControl control = new CellularLineControl(dev);
                    collection.Add(control);
                }
                catch
                {

                }
            }
            return collection;
        }
    }
}
