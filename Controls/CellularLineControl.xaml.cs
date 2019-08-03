using Windows.Devices.Sms;
using Windows.UI.Xaml.Controls;

namespace Chat.Controls
{
    public sealed partial class CellularLineControl : UserControl
    {
        public SmsDevice2 device;

        public CellularLineControl(SmsDevice2 device)
        {
            this.InitializeComponent();
            this.device = device;

            if (device != null)
            {
                if (device.AccountPhoneNumber != null)
                    LineName.Text = device.AccountPhoneNumber;

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
        }
    }
}
