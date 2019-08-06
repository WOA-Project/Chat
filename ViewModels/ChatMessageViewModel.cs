using Chat.Common;
using Chat.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Chat.ViewModels
{
    public class ChatMessageViewModel : Observable
    {
        private DateTime _timeStamp;
        public DateTime TimeStamp
        {
            get { return _timeStamp; }
            set { Set(ref _timeStamp, value); }
        }

        private string _messageBody;
        public string MessageBody
        {
            get { return _messageBody; }
            set { Set(ref _messageBody, value); }
        }

        private Contact _contact;
        public Contact Contact
        {
            get { return _contact; }
            set { Set(ref _contact, value); }
        }

        private HorizontalAlignment _alignment;
        public HorizontalAlignment Alignment
        {
            get { return _alignment; }
            set { Set(ref _alignment, value); }
        }

        private ChatMessageStore _store;
        private string _messageid;


        // Constructor
        public ChatMessageViewModel(string MessageId)
        {
            Initialize(MessageId);
        }

        // Initialize Stuff
        public async void Initialize(string MessageId)
        {
            if (string.IsNullOrEmpty(MessageId))
                return;

            _store = await ChatMessageManager.RequestStoreAsync();
            _messageid = MessageId;

            Contact = await GetContactInformation();
            (MessageBody, TimeStamp, Alignment) = await GetMessageInfo();

            _store.ChangeTracker.Enable();
            _store.StoreChanged += Store_StoreChanged;
        }

        // Methods
        private async Task<Contact> GetContactInformation()
        {
            var msg = await _store.GetMessageAsync(_messageid);

            if (!msg.IsIncoming)
                return await ContactUtils.GetMyself();

            return await ContactUtils.BindPhoneNumberToGlobalContact(msg.From);
        }

        private async Task<(string, DateTime, HorizontalAlignment)> GetMessageInfo()
        {
            var msg = await _store.GetMessageAsync(_messageid);
            var align = msg.IsIncoming ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            return (msg.Body, msg.LocalTimestamp.LocalDateTime, align);
        }

        private async void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            switch (args.Kind)
            {
                case ChatStoreChangedEventKind.MessageDeleted:
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            MessageBody = "Deleted message";
                        });
                        _store.StoreChanged -= Store_StoreChanged;
                        break;
                    }
                case ChatStoreChangedEventKind.MessageModified:
                    {
                        (var body, var ts, var align) = await GetMessageInfo();

                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            (MessageBody, TimeStamp) = (body, ts);
                        });
                        break;
                    }
            }
        }
    }
}
