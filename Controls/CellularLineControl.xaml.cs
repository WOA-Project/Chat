using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Calls;
using Windows.Devices.Enumeration;
using Windows.Devices.Sms;
using Windows.Networking.NetworkOperators;
using Windows.UI.Xaml.Controls;

namespace Chat.Controls
{
    public sealed partial class CellularLineControl : ComboBoxItem
    {
        public SmsDevice2 device;

        public CellularLineControl(SmsDevice2 device)
        {
            this.InitializeComponent();
            this.device = device;

            Load();
        }

        private async void Load()
        {
            string displayname = "";

            if (device != null)
            {
                if (device.AccountPhoneNumber != null)
                    displayname = device.AccountPhoneNumber;

                switch (device.DeviceStatus)
                {
                    case SmsDeviceStatus.DeviceBlocked:
                        {
                            StatusIcon.Text = "";
                            var pad = StatusIcon.Padding;
                            pad.Top = 0;
                            StatusIcon.Padding = pad;
                            break;
                        }
                    case SmsDeviceStatus.DeviceFailure:
                        {
                            StatusIcon.Text = "";
                            var pad = StatusIcon.Padding;
                            pad.Top = 0;
                            StatusIcon.Padding = pad;
                            break;
                        }
                    case SmsDeviceStatus.DeviceLocked:
                        {
                            StatusIcon.Text = "";
                            break;
                        }
                    case SmsDeviceStatus.Off:
                        {
                            StatusIcon.Text = "";
                            var pad = StatusIcon.Padding;
                            pad.Top = 0;
                            StatusIcon.Padding = pad;
                            break;
                        }
                    case SmsDeviceStatus.Ready:
                        {
                            StatusIcon.Text = "";
                            var pad = StatusIcon.Padding;
                            pad.Top = 0;
                            StatusIcon.Padding = pad;
                            break;
                        }
                    case SmsDeviceStatus.SimNotInserted:
                    case SmsDeviceStatus.SubscriptionNotActivated:
                    case SmsDeviceStatus.BadSim:
                        {
                            StatusIcon.Text = "";
                            break;
                        }
                }
            }

            try
            {
                string selectorStr = MobileBroadbandModem.GetDeviceSelector();
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selectorStr);

                foreach (var mdevice in devices)
                {
                    MobileBroadbandModem modem = MobileBroadbandModem.FromId(mdevice.Id);
                    if (modem.DeviceInformation.TelephoneNumbers.Count > 0)
                    {
                        if (modem.DeviceInformation.TelephoneNumbers.Any(x => x == device.AccountPhoneNumber))
                        {
                            displayname = modem.CurrentNetwork.RegisteredProviderName;

                            // from https://github.com/ADeltaX/MobileShell/blob/experiments/src/App.xaml.cs
                            PhoneCallStore store = await PhoneCallManager.RequestStoreAsync();
                            PhoneLineWatcher watcher = store.RequestLineWatcher();
                            List<PhoneLine> phoneLines = new List<PhoneLine>();
                            TaskCompletionSource<bool> lineEnumerationCompletion = new TaskCompletionSource<bool>();

                            watcher.LineAdded += async (o, args) => { var line = await PhoneLine.FromIdAsync(args.LineId); phoneLines.Add(line); };
                            watcher.Stopped += (o, args) => lineEnumerationCompletion.TrySetResult(false);
                            watcher.EnumerationCompleted += (o, args) => lineEnumerationCompletion.TrySetResult(true);

                            watcher.Start();

                            if (await lineEnumerationCompletion.Task)
                            {
                                watcher.Stop();

                                List<PhoneLine> returnedLines = new List<PhoneLine>();

                                foreach (PhoneLine phoneLine in phoneLines)
                                    if (phoneLine != null && phoneLine.Transport == PhoneLineTransport.Cellular)
                                        returnedLines.Add(phoneLine);

                                if (returnedLines.Any(x => x.NetworkName == modem.CurrentNetwork.RegisteredProviderName))
                                {
                                    var line = returnedLines.First(x => x.NetworkName == LineName.Text);
                                    displayname += " (SIM " + (line.CellularDetails.SimSlotIndex + 1) + ")";
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch
            {

            }

            LineName.Text = displayname;
        }
    }
}
