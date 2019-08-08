using Chat.Common;
using Chat.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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

        private Visibility _incomingVisibility;
        public Visibility IncomingVisibility
        {
            get { return _incomingVisibility; }
            set { Set(ref _incomingVisibility, value); }
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

        private ImageSource _image;
        public ImageSource Image
        {
            get { return _image; }
            set { Set(ref _image, value); }
        }

        private ChatMessageStore _store;
        private string _messageid;

        private bool mSubscribed;

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

            var _tmpContact = await GetContactInformation();
            (var _align, var _visi) = await GetMessageVisuals();

            (MessageBody, TimeStamp) = await GetMessageInfo();
            (Alignment, IncomingVisibility) = (_align, _visi);

            Contact = _tmpContact;

            var message = await _store.GetMessageAsync(_messageid);

            foreach (var attachment in message.Attachments)
            {
                try
                {
                    if (attachment.MimeType == "application/smil")
                    {

                    }

                    if (attachment.MimeType == "text/vcard")
                    {
                        
                    }

                    if (attachment.MimeType.StartsWith("image/"))
                    {
                        var imageextension = attachment.MimeType.Split('/').Last();
                        var img = new BitmapImage();
                        await img.SetSourceAsync(await attachment.DataStreamReference.OpenReadAsync());
                        Image = img;
                    }

                    if (attachment.MimeType.StartsWith("audio/"))
                    {
                        var audioextension = attachment.MimeType.Split('/').Last();
                    }
                }
                catch
                {
                    
                }
            }

            _store.ChangeTracker.Enable();
            Subscribe(true);
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
        private async Task<Contact> GetContactInformation()
        {
            var msg = await _store.GetMessageAsync(_messageid);

            if (!msg.IsIncoming)
                return await ContactUtils.GetMyself();

            return await ContactUtils.BindPhoneNumberToGlobalContact(msg.From);
        }

        private async Task<(HorizontalAlignment, Visibility)> GetMessageVisuals()
        {
            var msg = await _store.GetMessageAsync(_messageid);
            var align = msg.IsIncoming ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            var visi = msg.IsIncoming ? Visibility.Collapsed : Visibility.Visible;
            return (align, visi);
        }

        private async Task<(string, DateTime)> GetMessageInfo()
        {
            var msg = await _store.GetMessageAsync(_messageid);
            return (msg.Body, msg.LocalTimestamp.LocalDateTime);
        }

        private async void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            if (args.Id != _messageid)
                return;

            switch (args.Kind)
            {
                case ChatStoreChangedEventKind.MessageDeleted:
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            MessageBody = "Deleted message";
                        });
                        DropEvents();
                        break;
                    }
                case ChatStoreChangedEventKind.MessageModified:
                    {
                        (var body, var ts) = await GetMessageInfo();

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
