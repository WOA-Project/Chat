using Chat.ViewModels;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Windows.ApplicationModel.Chat;
using Windows.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Chat.Controls
{
    public sealed partial class ChatMessageViewControl : UserControl
    {
        public ChatMessageViewModel ViewModel { get; } = new ChatMessageViewModel("");

        public string messageId;
        public ChatMessageViewControl(string messageId)
        {
            this.InitializeComponent();
            this.messageId = messageId;
            ViewModel.Initialize(messageId);
        }

        private ICommand _messageDelete;
        public ICommand MessageDelete
        {
            get
            {
                if (_messageDelete == null)
                {
                    _messageDelete = new RelayCommand(
                        async () =>
                        {
                            var store = await ChatMessageManager.RequestStoreAsync();
                            await store.DeleteMessageAsync(messageId);
                        });
                }
                return _messageDelete;
            }
        }

        private ICommand _messageForward;
        public ICommand MessageForward
        {
            get
            {
                if (_messageForward == null)
                {
                    _messageForward = new RelayCommand(
                        () =>
                        {

                        });
                }
                return _messageForward;
            }
        }

        private ICommand _messageCopy;
        public ICommand MessageCopy
        {
            get
            {
                if (_messageCopy == null)
                {
                    _messageCopy = new RelayCommand(
                        async () =>
                        {
                            var store = await ChatMessageManager.RequestStoreAsync();
                            var msg = await store.GetMessageAsync(messageId);
                            var dataPackage = new DataPackage();
                            dataPackage.SetText(msg.Body);
                            Clipboard.SetContent(dataPackage);
                        });
                }
                return _messageCopy;
            }
        }

        private ICommand _messageDetails;
        public ICommand MessageDetails
        {
            get
            {
                if (_messageDetails == null)
                {
                    _messageDetails = new RelayCommand(
                        () =>
                        {

                        });
                }
                return _messageDetails;
            }
        }

        private void UserControl_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            messageMenuFlyout.ShowAt(this, e.GetPosition(this));
        }
    }
}
