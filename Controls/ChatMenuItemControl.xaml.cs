using Chat.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel.Chat;

namespace Chat.Controls
{
    public sealed partial class ChatMenuItemControl : NavigationViewItem
    {
        public ChatMenuItemViewModel ViewModel { get; } = new ChatMenuItemViewModel("");
        public string ConversationId
        {
            get; internal set;
        }

        public ChatMenuItemControl(string ConversationId)
        {
            InitializeComponent();

            this.ConversationId = ConversationId;
            ViewModel.Initialize(ConversationId);
        }

        private async void DeleteConvoButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();
            await (await store.GetConversationAsync(ConversationId)).DeleteAsync();
        }
    }
}
