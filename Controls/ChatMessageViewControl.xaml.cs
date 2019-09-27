using Chat.ViewModels;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Windows.ApplicationModel.Chat;
using Windows.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.Foundation;

namespace Chat.Controls
{
    public sealed partial class ChatMessageViewControl : UserControl
    {
        public ChatMessageViewModel ViewModel { get; } = new ChatMessageViewModel("");

        public string messageId { get; internal set; }
        public ChatMessageViewControl(string messageId)
        {
            this.InitializeComponent();
            this.messageId = messageId;
            ViewModel.Initialize(messageId);

            Loaded += ChatMessageViewControl_Loaded;
        }

        private void ChatMessageViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshVisuals();
        }

        public void RefreshVisuals()
        {
            var height = ((Shell)Window.Current.Content).ActualHeight;
            var width = ((Shell)Window.Current.Content).ActualWidth;

            var ttv = ChatBubble.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            var controlWidth = ChatBubble.Width;
            var controlHeight = ChatBubble.Height;

            BgColor.Width = width;
            BgColor.Height = height;

            var marg = BgColor.Margin;
            marg.Left = screenCoords.X - width;
            marg.Top = screenCoords.Y - height;
            marg.Right = screenCoords.X - width - controlWidth;
            marg.Bottom = screenCoords.Y - height - controlHeight;

            BgColor.Margin = marg;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private void UserControl_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            messageMenuFlyout.ShowAt(this, e.GetPosition(this));
        }
    }
}
