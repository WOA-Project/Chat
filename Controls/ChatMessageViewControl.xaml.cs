using Chat.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chat.Controls
{
    public sealed partial class ChatMessageViewControl : UserControl
    {
        public ChatMessageViewModel ViewModel { get; } = new ChatMessageViewModel("");

        public string messageId
        {
            get; internal set;
        }
        public ChatMessageViewControl(string messageId)
        {
            InitializeComponent();
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
            double height = ((Shell)Window.Current.Content).ActualHeight;
            double width = ((Shell)Window.Current.Content).ActualWidth;

            Windows.UI.Xaml.Media.GeneralTransform ttv = ChatBubble.TransformToVisual(Window.Current.Content);
            Point screenCoords = ttv.TransformPoint(new Point(0, 0));

            double controlWidth = ChatBubble.Width;
            double controlHeight = ChatBubble.Height;

            BgColor.Width = width;
            BgColor.Height = height;

            Thickness marg = BgColor.Margin;
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
                _messageDelete ??= new RelayCommand(
                        async () =>
                        {
                            ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();
                            await store.DeleteMessageAsync(messageId);
                        });
                return _messageDelete;
            }
        }

        private ICommand _messageForward;
        public ICommand MessageForward
        {
            get
            {
                _messageForward ??= new RelayCommand(
                        () =>
                        {

                        });
                return _messageForward;
            }
        }

        private ICommand _messageCopy;
        public ICommand MessageCopy
        {
            get
            {
                _messageCopy ??= new RelayCommand(
                        async () =>
                        {
                            ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();
                            ChatMessage msg = await store.GetMessageAsync(messageId);
                            DataPackage dataPackage = new();
                            dataPackage.SetText(msg.Body);
                            Clipboard.SetContent(dataPackage);
                        });
                return _messageCopy;
            }
        }

        private ICommand _messageDetails;
        public ICommand MessageDetails
        {
            get
            {
                _messageDetails ??= new RelayCommand(
                        () =>
                        {

                        });
                return _messageDetails;
            }
        }

        private void UserControl_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            messageMenuFlyout.ShowAt(this, e.GetPosition(this));
        }
    }
}
