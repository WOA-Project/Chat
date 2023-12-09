using Chat.Common;
using Chat.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Core;
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
            get => _timeStamp;
            set => Set(ref _timeStamp, value);
        }

        private Visibility _incomingVisibility;
        public Visibility IncomingVisibility
        {
            get => _incomingVisibility;
            set => Set(ref _incomingVisibility, value);
        }

        private string _messageBody;
        public string MessageBody
        {
            get => _messageBody;
            set => Set(ref _messageBody, value);
        }

        private Contact _contact;
        public Contact Contact
        {
            get => _contact;
            set => Set(ref _contact, value);
        }

        private HorizontalAlignment _alignment;
        public HorizontalAlignment Alignment
        {
            get => _alignment;
            set => Set(ref _alignment, value);
        }

        private ImageSource _image;
        public ImageSource Image
        {
            get => _image;
            set => Set(ref _image, value);
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
            {
                return;
            }

            _store = await ChatMessageManager.RequestStoreAsync();
            _messageid = MessageId;

            Contact _tmpContact = await GetContactInformation();
            (HorizontalAlignment _align, Visibility _visi) = await GetMessageVisuals();

            (MessageBody, TimeStamp) = await GetMessageInfo();
            (Alignment, IncomingVisibility) = (_align, _visi);

            Contact = _tmpContact;

            ChatMessage message = await _store.GetMessageAsync(_messageid);

            foreach (ChatMessageAttachment attachment in message.Attachments)
            {
                try
                {
                    if (attachment.MimeType == "application/smil")
                    {

                    }

                    if (attachment.MimeType == "text/vcard")
                    {

                    }

                    if (attachment.MimeType.StartsWith("image/", StringComparison.InvariantCulture))
                    {
                        string imageextension = attachment.MimeType.Split('/').Last();
                        BitmapImage img = new();
                        await img.SetSourceAsync(await attachment.DataStreamReference.OpenReadAsync());
                        Image = img;
                    }

                    if (attachment.MimeType.StartsWith("audio/", StringComparison.InvariantCulture))
                    {
                        string audioextension = attachment.MimeType.Split('/').Last();
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
        private async Task<Contact> GetContactInformation()
        {
            ChatMessage msg = await _store.GetMessageAsync(_messageid);

            return !msg.IsIncoming ? await ContactUtils.GetMyself() : await ContactUtils.BindPhoneNumberToGlobalContact(msg.From);
        }

        private async Task<(HorizontalAlignment, Visibility)> GetMessageVisuals()
        {
            ChatMessage msg = await _store.GetMessageAsync(_messageid);
            HorizontalAlignment align = msg.IsIncoming ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            Visibility visi = msg.IsIncoming ? Visibility.Collapsed : Visibility.Visible;
            return (align, visi);
        }

        private async Task<(string, DateTime)> GetMessageInfo()
        {
            ChatMessage msg = await _store.GetMessageAsync(_messageid);
            return (msg.Body, msg.LocalTimestamp.LocalDateTime);
        }

        private async void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            if (args.Id != _messageid)
            {
                return;
            }

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
                        (string body, DateTime ts) = await GetMessageInfo();

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
