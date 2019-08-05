using Chat.ContentDialogs;
using Chat.Controls;
using Chat.Pages;
using Chat.ViewModels;
using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private void NewConvoButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(ComposePage));
            ViewModel.SelectedItem = null;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem != null)
                MainFrame.Navigate(typeof(ConversationPage), (args.SelectedItem as ChatMenuItemControl).ChatConversation);
        }

        private async void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            await new AboutContentDialog().ShowAsync();
        }
    }
}
