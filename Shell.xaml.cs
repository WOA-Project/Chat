using Chat.Common;
using Chat.ContentDialogs;
using Chat.Controls;
using Chat.Pages;
using Chat.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Sms;
using Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace Chat
{
    public sealed partial class Shell : UserControl
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public Shell()
        {
            InitializeComponent();
        }

        public void HandleArguments(object e)
        {
            if (e is ProtocolActivatedEventArgs args)
            {
                HandleArguments(args);
            }
            if (e is ToastNotificationActivatedEventArgs args2)
            {
                HandleArguments(args2);
            }
        }

        private async void HandleArguments(ProtocolActivatedEventArgs args)
        {
            Uri uri = args.Uri;
            string unescpateduri = Uri.UnescapeDataString(uri.Query);
            string contactid = unescpateduri.Replace("?ContactRemoteIds=", "", StringComparison.InvariantCulture);
            if (uri.Scheme == "ms-ipmessaging")
            {
                ContactStore store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);
                Contact contact = await store.GetContactAsync(contactid);

                _ = MainFrame.Navigate(typeof(ComposePage), contact);
            }
        }

        private async void HandleArguments(ToastNotificationActivatedEventArgs args)
        {
            string arguments = args.Argument;

            string action = args.Argument.Split('&').First(x => x.ToLower(new CultureInfo("en-US")).StartsWith("action=", StringComparison.InvariantCulture)).Split('=')[1];
            string from = args.Argument.Split('&').First(x => x.ToLower(new CultureInfo("en-US")).StartsWith("from=", StringComparison.InvariantCulture)).Split('=')[1];
            string deviceid = args.Argument.Split('&').First(x => x.ToLower(new CultureInfo("en-US")).StartsWith("deviceid=", StringComparison.InvariantCulture)).Split('=')[1];

            switch (action.ToLower(new CultureInfo("en-US")))
            {
                case "reply":
                    {
                        try
                        {
                            string messagetosend = (string)args.UserInput["textBox"];
                            SmsDevice2 smsDevice = SmsDevice2.FromId(deviceid);
                            _ = await SmsUtils.SendTextMessageAsync(smsDevice, from, messagetosend);
                        }
                        catch
                        {

                        }

                        break;
                    }
                case "openthread":
                    {
                        ChatMenuItemControl selectedConvo = null;
                        foreach (ChatMenuItemControl convo in ViewModel.ChatConversations)
                        {
                            Contact contact = convo.ViewModel.Contact;
                            foreach (ContactPhone num in contact.Phones)
                            {
                                if (ContactUtils.ArePhoneNumbersMostLikelyTheSame(from, num.Number))
                                {
                                    selectedConvo = convo;
                                    break;
                                }
                            }
                            if (selectedConvo != null)
                            {
                                break;
                            }
                        }
                        if (selectedConvo != null)
                        {
                            ViewModel.SelectedItem = selectedConvo;
                        }

                        break;
                    }
            }
        }

        private ICommand _newConvoCommand;
        public ICommand NewConvoCommand
        {
            get
            {
                _newConvoCommand ??= new RelayCommand(
                        () =>
                        {
                            _ = MainFrame.Navigate(typeof(ComposePage));
                            ViewModel.SelectedItem = null;
                        });
                return _newConvoCommand;
            }
        }

        private ICommand _openAboutCommand;
        public ICommand OpenAboutCommand
        {
            get
            {
                _openAboutCommand ??= new RelayCommand(
                        async () =>
                        {
#if !DEBUG
                            await new AboutContentDialog().ShowAsync();
#else
                            Windows.ApplicationModel.Chat.ChatMessageStore store = await Windows.ApplicationModel.Chat.ChatMessageManager.RequestStoreAsync();
                            string transportId = await Windows.ApplicationModel.Chat.ChatMessageManager.RegisterTransportAsync();
                            Windows.ApplicationModel.Chat.ChatMessage msg = new()
                            {
                                Body = "Hello how are you?",

                                TransportId = transportId,

                                MessageOperatorKind = Windows.ApplicationModel.Chat.ChatMessageOperatorKind.Sms,
                                Status = Windows.ApplicationModel.Chat.ChatMessageStatus.Sent
                            };

                            bool alternate = new Random().Next(2) == 2;

                            msg.Recipients.Clear();
                            msg.From = "";

                            DateTimeOffset offset = new(DateTime.Now);
                            msg.LocalTimestamp = offset;
                            msg.NetworkTimestamp = offset;

                            msg.IsIncoming = alternate;
                            if (msg.IsIncoming)
                            {
                                msg.From = "Random dude";
                            }
                            else
                            {
                                msg.Recipients.Add("Random dude");
                            }

                            alternate = !alternate;

                            await store.SaveMessageAsync(msg);
#endif
                        });
                return _openAboutCommand;
            }
        }

        private void NavigationView_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem != null)
            {
                _ = MainFrame.Navigate(typeof(ConversationPage), (args.SelectedItem as ChatMenuItemControl).ConversationId);
            }
        }
    }
}
