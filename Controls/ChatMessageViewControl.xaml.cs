using Chat.ViewModels;
using Windows.UI.Xaml.Controls;

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
    }
}
