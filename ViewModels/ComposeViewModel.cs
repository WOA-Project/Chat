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
            internal set { Set(ref _cellularLines, value); }
        }

        private CellularLineControl _selectedLine;
        public CellularLineControl SelectedLine
        {
            get { return _selectedLine; }
            set { Set(ref _selectedLine, value); }
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

            if (CellularLines.Count != 0)
                SelectedLine = CellularLines[0];
        }

        // Methods
        private static async Task<ObservableCollection<CellularLineControl>> GetSmsDevices()
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
