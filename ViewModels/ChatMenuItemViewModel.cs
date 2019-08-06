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
            get { return _contact; }
            set { Set(ref _contact, value); }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set { Set(ref _displayName, value); }
        }

        private string _displayDescription;
        public string DisplayDescription
        {
            get { return _displayDescription; }
            set { Set(ref _displayDescription, value); }
        }

        private DateTime _timeStamp;
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { Set(ref _timeStamp, value); }
        }

        private ChatMessageStore _store;
        private string _conversationid;

        // Constructor
        public ChatMenuItemViewModel(string ConvoId)
        {
            Initialize(ConvoId);
        }


        // Initialize Stuff
        public async void Initialize(string ConvoId)
        {
            if (string.IsNullOrEmpty(ConvoId))
                return;

            _store = await ChatMessageManager.RequestStoreAsync();
            _conversationid = ConvoId;

            (Contact, DisplayName) = await GetContactInformation();
            (DisplayDescription, TimeStamp) = await GetLastMessageInfo();

            _store.ChangeTracker.Enable();
            _store.StoreChanged += Store_StoreChanged;
        }

        // Methods
        private async Task<(Contact, string)> GetContactInformation()
        {
            var convo = await _store.GetConversationAsync(_conversationid);
            var contact = await ContactUtils.BindPhoneNumberToGlobalContact(convo.Participants.First());

            return (contact, contact.DisplayName);
        }

        private async Task<(string, DateTime)> GetLastMessageInfo()
        {
            var convo = await _store.GetConversationAsync(_conversationid);

            var messageReader = convo.GetMessageReader();
            var lastMessageId = convo.MostRecentMessageId;

            var messages = await messageReader.ReadBatchAsync();

            var lastMessage = messages.Where(x => x.Id == lastMessageId).First();

            return (lastMessage.Body, lastMessage.LocalTimestamp.LocalDateTime);
        }

        private async void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            switch (args.Kind)
            {
                case ChatStoreChangedEventKind.ConversationModified:
                    {
                        var conversation = await _store.GetConversationAsync(args.Id);

                        if (conversation == null)
                            break;

                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            Initialize(_conversationid);
                        });
                        break;
                    }
            }
        }
    }
}
