using Chat.Common;
using Chat.Controls;
using Chat.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace Chat.Pages
{
    public sealed partial class ComposePage : Page
    {
        public ComposeViewModel ViewModel { get; } = new ComposeViewModel();

        public ComposePage()
        {
            InitializeComponent();
            Loaded += ComposePage_Loaded;
        }

        private void ComposePage_Loaded(object sender, RoutedEventArgs e)
        {
            _ = ContactPickerBox.Focus(FocusState.Pointer);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e != null)
            {
                if (e.Parameter is Contact args)
                {
                    ContactPickerBox.Text = args.DisplayName;
                }
            }
        }

        private async void ContactPickerBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ContactStore store = await ContactManager.RequestStoreAsync();
            List<ContactPhoneViewControl> contactControls = [];

            try
            {
                if (!sender.Text.Contains(";", StringComparison.InvariantCulture))
                {
                    IReadOnlyList<Contact> contacts = await store.FindContactsAsync(sender.Text);

                    if (contacts != null)
                    {
                        IEnumerable<Contact> phonecontacts = contacts.Where(x => x.Phones.Count != 0);
                        if (phonecontacts != null)
                        {
                            foreach (Contact phonecontact in phonecontacts)
                            {
                                foreach (ContactPhone phone in phonecontact.Phones)
                                {
                                    ContactPhoneViewControl control = new(phone, phonecontact);
                                    contactControls.Add(control);
                                }
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
                ContactPicker picker = new()
                {
                    CommitButtonText = "Select",
                    SelectionMode = ContactSelectionMode.Fields
                };
                picker.DesiredFieldsWithContactFieldType.Add(ContactFieldType.PhoneNumber);

                Contact result = await picker.PickContactAsync();
                if (result != null)
                {
                    if (string.IsNullOrEmpty(ContactPickerBox.Text))
                    {
                        ContactPickerBox.Text = result.Phones.First().Number;
                    }
                    else
                    {
                        ContactPickerBox.Text += "; " + result.Phones.First().Number;
                    }

                    _ = ContactPickerBox.Focus(FocusState.Pointer);
                }
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

        private ICommand _showAttachments;
        public ICommand ShowAttachments
        {
            get
            {
                _showAttachments ??= new RelayCommand(
                        () =>
                        {
                            FlyoutBase.ShowAttachedFlyout(AttachmentButton);
                        });
                return _showAttachments;
            }
        }


        private ICommand _sendReply;
        public ICommand SendReply
        {
            get
            {
                _sendReply ??= new RelayCommand(
                        async () =>
                        {
                            SendButton.IsEnabled = false;
                            ComposeTextBox.IsEnabled = false;
                            Windows.Devices.Sms.SmsDevice2 smsDevice = ViewModel.SelectedLine.device;

                            try
                            {
                                bool result = await SmsUtils.SendTextMessageAsync(smsDevice, ContactPickerBox.Text.Split(';'), ComposeTextBox.Text);
                                if (!result)
                                {
                                    _ = await new MessageDialog("We could not send one or some messages.", "Something went wrong").ShowAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                _ = await new MessageDialog($"We could not send one or some messages.\n{ex}", "Something went wrong").ShowAsync();
                            }

                            SendButton.IsEnabled = true;
                            ComposeTextBox.IsEnabled = true;
                            ComposeTextBox.Text = "";
                        });
                return _sendReply;
            }
        }
    }
}
