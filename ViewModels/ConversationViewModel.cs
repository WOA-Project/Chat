using Chat.Common;
using Chat.Controls;
using Chat.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Enumeration;
using Windows.Devices.Sms;

namespace Chat.ViewModels
{
    public class ConversationViewModel : Observable
    {
        // Properties
        private ObservableCollection<ChatMessageViewControl> _chatMessages;
        public ObservableCollection<ChatMessageViewControl> ChatMessages
        {
            get { return _chatMessages; }
            set { Set(ref _chatMessages, value); }
        }

        private ObservableCollection<CellularLineControl> _cellularLines;
        public ObservableCollection<CellularLineControl> CellularLines
        {
            get { return _cellularLines; }
            set { Set(ref _cellularLines, value); }
        }

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

        private ChatMessageStore _store;
        private string _conversationid;

        // Constructor
        public ConversationViewModel(string ConvoId)
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

            ChatMessages = await GetMessages();
            CellularLines = await GetSmsDevices();
            (Contact, DisplayName) = await GetContactInformation();

            _store.ChangeTracker.Enable();
            _store.StoreChanged += Store_StoreChanged;
        }

        // Methods
        private async Task<ObservableCollection<ChatMessageViewControl>> GetMessages()
        {
            ObservableCollection<ChatMessageViewControl> collection = new ObservableCollection<ChatMessageViewControl>();

            var convo = await _store.GetConversationAsync(_conversationid);
            var reader = convo.GetMessageReader();
            var messages = await reader.ReadBatchAsync();

            messages.ToList().ForEach(x => collection.Insert(0, new ChatMessageViewControl(x)));

            return collection;
        }

        private async Task<(Contact, string)> GetContactInformation()
        {
            var convo = await _store.GetConversationAsync(_conversationid);
            var contact = await ContactUtils.BindPhoneNumberToGlobalContact(convo.Participants.First());

            return (contact, contact.DisplayName);
        }

        private async Task<ObservableCollection<CellularLineControl>> GetSmsDevices()
        {
            ObservableCollection<CellularLineControl> collection = new ObservableCollection<CellularLineControl>();
            var smsDevices = await DeviceInformation.FindAllAsync(SmsDevice2.GetDeviceSelector(), null);
            foreach (var smsDevice in smsDevices)
            {
                try
                {
                    SmsDevice2 dev = SmsDevice2.FromId(smsDevice.Id);
                    CellularLineControl control = new CellularLineControl(dev);
                    collection.Add(control);
                }
                catch
                {

                }
            }
            return collection;
        }

        private void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            switch (args.Kind)
            {
                case ChatStoreChangedEventKind.MessageCreated:
                case ChatStoreChangedEventKind.MessageDeleted:
                case ChatStoreChangedEventKind.MessageModified:
                    {
                        break;
                    }
            }
        }
    }
}
