using Chat.Common;
using Chat.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Chat.ViewModels
{
    public class ChatMenuItemViewModel : Observable
    {

        private Contact _contact;
        public Contact Contact
        {
            get => _contact;
            set => Set(ref _contact, value);
        }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => Set(ref _displayName, value);
        }

        private string _displayDescription;
        public string DisplayDescription
        {
            get => _displayDescription;
            set => Set(ref _displayDescription, value);
        }

        private DateTime _timeStamp;
        public DateTime TimeStamp
        {
            get => _timeStamp;
            set => Set(ref _timeStamp, value);
        }

        private ChatMessageStore _store;
        private string _conversationid;

        private bool mSubscribed;

        // Constructor
        public ChatMenuItemViewModel(string ConvoId)
        {
            Initialize(ConvoId);
        }


        // Initialize Stuff
        public async void Initialize(string ConvoId)
        {
            if (string.IsNullOrEmpty(ConvoId))
            {
                return;
            }

            _store = await ChatMessageManager.RequestStoreAsync();
            _conversationid = ConvoId;

            ChatConversation convo = await _store.GetConversationAsync(_conversationid);
            if (convo == null)
            {
                DropEvents();
                return;
            }

            (Contact tmpContact, string tmpDisplayName) = await GetContactInformation();
            (DisplayDescription, TimeStamp) = await GetLastMessageInfo();
            (Contact, DisplayName) = (tmpContact, tmpDisplayName);

            _store.ChangeTracker.Enable();
            Subscribe(true);
        }

        private void Subscribe(bool enabled)
        {
            if (!enabled && mSubscribed)
            {
                _store.StoreChanged -= Store_StoreChanged;
            }
            else if (enabled && !mSubscribed)
            {
                _store.StoreChanged += Store_StoreChanged;
            }

            mSubscribed = enabled;
        }

        public void DropEvents()
        {
            Subscribe(false);
        }


        // Methods
        private async Task<(Contact, string)> GetContactInformation()
        {
            try
            {
                ChatConversation convo = await _store.GetConversationAsync(_conversationid);
                Contact contact = await ContactUtils.BindPhoneNumberToGlobalContact(convo.Participants.First());

                return (contact, contact.DisplayName);
            }
            catch
            {
                Contact blankcontact = new();
                blankcontact.Phones.Add(new ContactPhone() { Number = "Unknown", Kind = ContactPhoneKind.Other });
                return (blankcontact, "Unknown");
            }
        }

        private async Task<(string, DateTime)> GetLastMessageInfo()
        {
            ChatConversation convo = await _store.GetConversationAsync(_conversationid);

            ChatMessageReader messageReader = convo.GetMessageReader();
            string lastMessageId = convo.MostRecentMessageId;

            System.Collections.Generic.IReadOnlyList<ChatMessage> messages = await messageReader.ReadBatchAsync();

            ChatMessage lastMessage = messages.Where(x => x.Id == lastMessageId).First();

            return (lastMessage.Body, lastMessage.LocalTimestamp.LocalDateTime);
        }

        private async void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            if (args.Id != _conversationid)
            {
                return;
            }

            switch (args.Kind)
            {
                case ChatStoreChangedEventKind.ConversationModified:
                    {
                        ChatConversation conversation = await _store.GetConversationAsync(args.Id);

                        if (conversation == null)
                        {
                            DropEvents();
                            break;
                        }

                        (string str, DateTime dt) = await GetLastMessageInfo();

                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            (DisplayDescription, TimeStamp) = (str, dt);
                        });
                        break;
                    }
                case ChatStoreChangedEventKind.ConversationDeleted:
                    {
                        DropEvents();
                        break;
                    }
            }
        }
    }
}
