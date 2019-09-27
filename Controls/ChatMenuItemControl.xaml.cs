using Chat.ViewModels;
using System;
using Windows.ApplicationModel.Chat;
using Microsoft.UI.Xaml.Controls;

namespace Chat.Controls
{
    public sealed partial class ChatMenuItemControl : NavigationViewItem
    {
        public ChatMenuItemViewModel ViewModel { get; } = new ChatMenuItemViewModel("");
        public string ConversationId { get; internal set; }

        public ChatMenuItemControl(string ConversationId)
        {
            this.InitializeComponent();

            this.ConversationId = ConversationId;
            ViewModel.Initialize(ConversationId);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private async void DeleteConvoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var store = await ChatMessageManager.RequestStoreAsync();
            await (await store.GetConversationAsync(ConversationId)).DeleteAsync();
        }
    }
}
