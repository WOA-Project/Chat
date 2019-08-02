using Chat.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Enumeration;
using Windows.Devices.Sms;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Chat.Pages
{
    public sealed partial class ComposePage : Page
    {
        public ComposePage()
        {
            this.InitializeComponent();;
            Load();
            Loaded += ComposePage_Loaded;
        }

        private void ComposePage_Loaded(object sender, RoutedEventArgs e)
        {
            ContactPickerBox.Focus(FocusState.Pointer);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var args = e.Parameter as Contact;
            if (args != null)
            {
                ContactPickerBox.Text = args.DisplayName;
            }
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

        private async void ContactPickerBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var store = await ContactManager.RequestStoreAsync();
            List<ContactPhoneViewControl> contactControls = new List<ContactPhoneViewControl>();

            try
            {
                var contacts = await store.FindContactsAsync(sender.Text);

                if (contacts != null)
                {
                    var phonecontacts = contacts.Where(x => x.Phones.Count != 0);
                    if (phonecontacts != null)
                    {
                        foreach (var phonecontact in phonecontacts)
                        {
                            foreach (var phone in phonecontact.Phones)
                            {
                                var control = new ContactPhoneViewControl(phone, phonecontact);
                                contactControls.Add(control);
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            sender.ItemsSource = contactControls;
        }

        private async void ContactPickerBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion == null)
            {
                var picker = new ContactPicker();
                picker.CommitButtonText = "Select";
                picker.SelectionMode = ContactSelectionMode.Fields;
                picker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.PhoneNumber);

                var result = await picker.PickContactAsync();
                ContactPickerBox.Text = result.DisplayName;
                ContactPickerBox.Focus(FocusState.Pointer);
            }
        }

        private void ContactPickerBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ContactPickerBox.Text = (args.SelectedItem as ContactPhoneViewControl).contactPhone.Number;
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
                var smsDevice = cellularlineControls[CellularLineComboBox.SelectedIndex].device;

                SmsTextMessage2 message = new SmsTextMessage2();
                message.Body = ComposeTextBox.Text;

                foreach (var number in ContactPickerBox.Text.Split(';'))
                {
                    message.To = number.TrimStart().TrimEnd();

                    try
                    {
                        SmsSendMessageResult result = await smsDevice.SendMessageAndGetResultAsync(message);
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message + " - " + ex.StackTrace).ShowAsync();
            }
        }
    }
}
