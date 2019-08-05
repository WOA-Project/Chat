using Chat.Helpers;
using Windows.ApplicationModel.Chat;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Chat.Controls
{
    public sealed partial class ChatMessageViewControl : UserControl
    {
        private ChatMessage message;
        public ChatMessageViewControl(ChatMessage message)
        {
            this.InitializeComponent();
            this.message = message;
            if (!message.IsIncoming)
            {
                ChatBubble.HorizontalAlignment = HorizontalAlignment.Right;
                BgColor.Opacity = 0.75;
            }
            DateTimeLabel.Text = message.LocalTimestamp.ToLocalTime().ToRelativeString();
        }
    }
}
