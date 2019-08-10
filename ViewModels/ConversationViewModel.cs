using Chat.Common;
using Chat.Controls;
using Chat.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.Sms;
using Windows.UI.Core;

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

        private CellularLineControl _selectedLine;
        public CellularLineControl SelectedLine
        {
            get { return _selectedLine; }
            set { Set(ref _selectedLine, value); }
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

        private bool mSubscribed;

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

            if (ChatMessages != null && ChatMessages.Count != 0)
            {
                foreach (var msg in ChatMessages)
                {
                    msg.ViewModel.DropEvents();
                }
            }

            if (ConvoId != _conversationid)
            {
                DropEvents();
            }

            _store = await ChatMessageManager.RequestStoreAsync();
            _conversationid = ConvoId;

            var _tmpchatMessages = await GetMessages();
            var _tmpCellLines = await GetSmsDevices();

            (Contact, DisplayName) = await GetContactInformation();
            ChatMessages = _tmpchatMessages;
            ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
            CellularLines = _tmpCellLines;

            if (CellularLines.Count != 0)
                SelectedLine = CellularLines[0];

            _store.ChangeTracker.Enable();
            Subscribe(true);
        }

        private void ChatMessages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in ChatMessages)
            {
                item.RefreshVisuals();
            }
        }

        private void Subscribe(bool enabled)
        {
            if (!enabled && mSubscribed) _store.StoreChanged -= Store_StoreChanged;
            else if (enabled && !mSubscribed) _store.StoreChanged += Store_StoreChanged;

            mSubscribed = enabled;
        }

        public void DropEvents()
        {
            Subscribe(false);
        }

        // Methods
        private async Task<ObservableCollection<ChatMessageViewControl>> GetMessages()
        {
            ObservableCollection<ChatMessageViewControl> collection = new ObservableCollection<ChatMessageViewControl>();

            var convo = await _store.GetConversationAsync(_conversationid);
            var reader = convo.GetMessageReader();
            var messages = await reader.ReadBatchAsync();

            messages.ToList().ForEach(x => collection.Insert(0, new ChatMessageViewControl(x.Id)));

            return collection;
        }

        private async void UpdateMessages()
        {
            var convo = await _store.GetConversationAsync(_conversationid);
            if (convo == null)
            {
                DropEvents();
                return;
            }

            var reader = convo.GetMessageReader();
            var messages = await reader.ReadBatchAsync();

            var currindex = ChatMessages.Count();

            foreach (var message in messages)
            {
                if (ChatMessages.Any(x => x.messageId == message.Id))
                {
                    break;
                }
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    ChatMessages.Insert(currindex, new ChatMessageViewControl(message.Id));
                });
            }
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

        private async void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            if (args.Id != _conversationid)
                return;

            switch (args.Kind)
            {
                case ChatStoreChangedEventKind.ConversationModified:
                    {
                        var conversation = await _store.GetConversationAsync(args.Id);

                        if (conversation == null)
                        {
                            DropEvents();
                            break;
                        }

                        UpdateMessages();
                        break;
                    }
            }
        }
    }
}
