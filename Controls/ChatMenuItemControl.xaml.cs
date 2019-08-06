using Chat.ViewModels;
using System;
using Windows.ApplicationModel.Chat;
using Windows.UI.Xaml.Controls;

namespace Chat.Controls
{
    public sealed partial class ChatMenuItemControl : NavigationViewItem
    {
        public ChatMenuItemViewModel ViewModel { get; } = new ChatMenuItemViewModel("");
        public string ConversationId;

        public ChatMenuItemControl(string ConversationId)
        {
            this.InitializeComponent();

            this.ConversationId = ConversationId;
            ViewModel.Initialize(ConversationId);
        }

        private async void DeleteConvoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var store = await ChatMessageManager.RequestStoreAsync();
            await (await store.GetConversationAsync(ConversationId)).DeleteAsync();
        }
    }
}
