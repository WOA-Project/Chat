using Chat.ContentDialogs;
using Chat.Controls;
using Chat.Pages;
using Chat.ViewModels;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml.Controls;
using MUXC = Microsoft.UI.Xaml.Controls;

namespace Chat
{
    public sealed partial class Shell : UserControl
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public Shell()
        {
            this.InitializeComponent();
        }

        public void HandleArguments(object e)
        {
            var args = e as ProtocolActivatedEventArgs;
            if (args != null)
            {
                HandleArguments(args);
            }
        }

        private async void HandleArguments(ProtocolActivatedEventArgs args)
        {
            Uri uri = args.Uri;
            string unescpateduri = Uri.UnescapeDataString(uri.Query);
            var contactid = unescpateduri.Replace("?ContactRemoteIds=", "");
            if (uri.Scheme == "ms-ipmessaging")
            {
                ContactStore store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);
                Contact contact = await store.GetContactAsync(contactid);

                MainFrame.Navigate(typeof(ComposePage), contact);
            }
        }

        private ICommand _newConvoCommand;
        public ICommand NewConvoCommand
        {
            get
            {
                if (_newConvoCommand == null)
                {
                    _newConvoCommand = new RelayCommand(
                        () =>
                        {
                            MainFrame.Navigate(typeof(ComposePage));
                            ViewModel.SelectedItem = null;
                        });
                }
                return _newConvoCommand;
            }
        }

        private ICommand _openAboutCommand;
        public ICommand OpenAboutCommand
        {
            get
            {
                if (_openAboutCommand == null)
                {
                    _openAboutCommand = new RelayCommand(
                        async () =>
                        {
#if !DEBUG
                            await new AboutContentDialog().ShowAsync();
#else
                            Windows.ApplicationModel.Chat.ChatMessageStore store = await Windows.ApplicationModel.Chat.ChatMessageManager.RequestStoreAsync();
                            string transportId = await Windows.ApplicationModel.Chat.ChatMessageManager.RegisterTransportAsync();
                            Windows.ApplicationModel.Chat.ChatMessage msg = new Windows.ApplicationModel.Chat.ChatMessage();

                            msg.Body = "Hello how are you?";

                            msg.TransportId = transportId;

                            msg.MessageOperatorKind = Windows.ApplicationModel.Chat.ChatMessageOperatorKind.Sms;
                            msg.Status = Windows.ApplicationModel.Chat.ChatMessageStatus.Sent;

                            bool alternate = new Random().Next(2) == 2;

                            msg.Recipients.Clear();
                            msg.From = "";

                            var offset = new DateTimeOffset(DateTime.Now);
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
                }
                return _openAboutCommand;
            }
        }

        private void NavigationView_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem != null)
                MainFrame.Navigate(typeof(ConversationPage), (args.SelectedItem as ChatMenuItemControl).ConversationId);
        }
    }
}
