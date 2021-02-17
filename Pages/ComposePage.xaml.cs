using Chat.Common;
using Chat.Controls;
using Chat.Helpers;
using Chat.ViewModels;
using GalaSoft.MvvmLight.Command;
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
            this.InitializeComponent();
            Loaded += ComposePage_Loaded;
        }

        private void ComposePage_Loaded(object sender, RoutedEventArgs e)
        {
            ContactPickerBox.Focus(FocusState.Pointer);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e != null)
            {
                var args = e.Parameter as Contact;
                if (args != null)
                {
                    ContactPickerBox.Text = args.DisplayName;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private async void ContactPickerBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var store = await ContactManager.RequestStoreAsync();
            List<ContactPhoneViewControl> contactControls = new List<ContactPhoneViewControl>();

            try
            {
                if (!sender.Text.Contains(";", StringComparison.InvariantCulture))
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
                if (result != null)
                {
                    if (string.IsNullOrEmpty(ContactPickerBox.Text))
                        ContactPickerBox.Text = result.Phones.First().Number;
                    else
                        ContactPickerBox.Text += "; " + result.Phones.First().Number;
                    ContactPickerBox.Focus(FocusState.Pointer);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private void ContactPickerBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ContactPickerBox.Text = (args.SelectedItem as ContactPhoneViewControl).contactPhone.Number;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private void CellularLineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CellularLineComboBox.SelectedItem != null)
            {
                SendButton.IsEnabled = !string.IsNullOrEmpty(ComposeTextBox.Text);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
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
                if (_showAttachments == null)
                {
                    _showAttachments = new RelayCommand(
                        () =>
                        {
                            FlyoutBase.ShowAttachedFlyout((FrameworkElement)AttachmentButton);
                        });
                }
                return _showAttachments;
            }
        }


        private ICommand _sendReply;
        public ICommand SendReply
        {
            get
            {
                if (_sendReply == null)
                {
                    _sendReply = new RelayCommand(
                        async () =>
                        {
                            SendButton.IsEnabled = false;
                            ComposeTextBox.IsEnabled = false;
                            var smsDevice = ViewModel.SelectedLine.device;

                            try
                            {
                                var result = await SmsUtils.SendTextMessageAsync(smsDevice, ContactPickerBox.Text.Split(';'), ComposeTextBox.Text);
                                if (!result)
                                    await new MessageDialog("We could not send one or some messages.", "Something went wrong").ShowAsync();
                            }
                            catch (Exception ex)
                            {
                                await new MessageDialog($"We could not send one or some messages.\n{ex}", "Something went wrong").ShowAsync();
                            }

                            SendButton.IsEnabled = true;
                            ComposeTextBox.IsEnabled = true;
                            ComposeTextBox.Text = "";
                        });
                }
                return _sendReply;
            }
        }
    }
}
