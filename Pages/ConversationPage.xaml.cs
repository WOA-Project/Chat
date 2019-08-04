using Chat.Common;
using Chat.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Chat;
using Windows.Devices.Enumeration;
using Windows.Devices.Sms;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace Chat.Pages
{
    public sealed partial class ConversationPage : Page
    {
        private ChatConversation convo;
        private ObservableCollection<ChatMessageViewControl> observableCollection = new ObservableCollection<ChatMessageViewControl>();

        public ConversationPage()
        {
            this.InitializeComponent();
        }

        private List<CellularLineControl> cellularlineControls = new List<CellularLineControl>();

        private async void Load()
        {
            var smsDevices = await DeviceInformation.FindAllAsync(SmsDevice2.GetDeviceSelector(), null);
            foreach (var smsDevice in smsDevices)
            {
                try
                {
                    SmsDevice2 dev = SmsDevice2.FromId(smsDevice.Id);
                    CellularLineControl control = new CellularLineControl(dev);
                    cellularlineControls.Add(control);
                    CellularLineComboBox.Items.Add(new ComboBoxItem() { Content = control });
                }
                catch
                {

                }
            }
        }


        private void CellularLineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CellularLineComboBox.SelectedItem != null)
            {
                SendButton.IsEnabled = !string.IsNullOrEmpty(ComposeTextBox.Text);
            }
        }

        private void ComposeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CellularLineComboBox.SelectedItem != null)
            {
                SendButton.IsEnabled = !string.IsNullOrEmpty(ComposeTextBox.Text);
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SendButton.IsEnabled = false;
                var smsDevice = cellularlineControls[CellularLineComboBox.SelectedIndex].device;

                var result = await SmsUtils.SendTextMessageAsync(smsDevice, convo.Participants.First(), ComposeTextBox.Text);
                if (!result)
                    await new MessageDialog("We could not send one or some messages.", "Something went wrong").ShowAsync();

                SendButton.IsEnabled = true;
                ComposeTextBox.Text = "";
            }
            catch (Exception ex)
            {
                SendButton.IsEnabled = true;
                await new MessageDialog(ex.Message + " - " + ex.StackTrace).ShowAsync();
            }
        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var args = e.Parameter as ChatConversation;
            if (args != null)
            {
                convo = args;
            }

            var contact = await ContactUtils.BindPhoneNumberToGlobalContact(convo.Participants.First());
            ConvoTitle.Text = contact.DisplayName;
            ConvoPic.Contact = contact;

            var reader = convo.GetMessageReader();

            var messages = await reader.ReadBatchAsync();

            messages.ToList().ForEach(x => observableCollection.Insert(0, new ChatMessageViewControl(x)));

            Load();
        }

        private async void CallButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("tel:" + convo.Participants.First()));
        }
    }
}
