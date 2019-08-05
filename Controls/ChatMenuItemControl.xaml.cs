using Chat.Common;
using Chat.Helpers;
using System;
using System.Linq;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml.Controls;

namespace Chat.Controls
{
    public sealed partial class ChatMenuItemControl : NavigationViewItem
    {
        private string DisplayMessage;
        private Contact Contact;
        private string DisplayDate;
        private string DisplayName;

        public ChatConversation ChatConversation;

        public ChatMenuItemControl(ChatConversation ChatConversation)
        {
            this.InitializeComponent();

            this.ChatConversation = ChatConversation;
            Load();
        }

        private async void Load()
        {
            var participant = ChatConversation.Participants.First();
            var contact = await ContactUtils.BindPhoneNumberToGlobalContact(participant);

            var messageReader = ChatConversation.GetMessageReader();
            var lastMessageId = ChatConversation.MostRecentMessageId;

            var messages = await messageReader.ReadBatchAsync();

            var lastMessage = messages.Where(x => x.Id == lastMessageId).First();

            DisplayMessage = lastMessage.Body;
            DisplayDate = lastMessage.LocalTimestamp.ToLocalTime().ToRelativeString();
            Contact = contact;
            DisplayName = contact.DisplayName;

            ChatName.Text = DisplayName;
            ChatDate.Text = DisplayDate;
            PeoplePic.Contact = Contact;
            ChatContent.Text = DisplayMessage;
        }

        private async void DeleteConvoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ChatConversation.DeleteAsync();
        }
    }
}
