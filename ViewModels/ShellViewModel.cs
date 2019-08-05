using Chat.BackgroundTasks;
using Chat.Common;
using Chat.Controls;
using Chat.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Chat.ViewModels
{
    public class ShellViewModel : Observable
    {
        // Properties
        private ObservableCollection<ChatMenuItemControl> _chatConversations;
        public ObservableCollection<ChatMenuItemControl> ChatConversations
        {
            get { return _chatConversations; }
            set { Set(ref _chatConversations, value); }
        }

        private ChatMenuItemControl _selectedItem;
        public ChatMenuItemControl SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }

        private ChatMessageStore _store;

        // Constructor
        public ShellViewModel()
        {
            Initialize();
        }


        // Initialize Stuff
        public async void Initialize()
        {
            BackgroundTaskUtils.RegisterToastNotificationBackgroundTasks();
            ContactUtils.AssignAppToPhoneContacts();
            _store = await ChatMessageManager.RequestStoreAsync();

            ChatConversations = await GetChats();
            if (ChatConversations.Count != 0)
                SelectedItem = ChatConversations[0];

            _store.ChangeTracker.Enable();
            _store.StoreChanged += Store_StoreChanged;
        }

        // Methods
        private async Task<ObservableCollection<ChatMenuItemControl>> GetChats()
        {
            ObservableCollection<ChatMenuItemControl> collection = new ObservableCollection<ChatMenuItemControl>();

            var reader = _store.GetConversationReader();
            var convos = await reader.ReadBatchAsync();

            foreach (var convo in convos)
            {
                collection.Add(new ChatMenuItemControl(convo));
            }

            return collection;
        }

        private async void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            switch (args.Kind)
            {
                case ChatStoreChangedEventKind.ConversationModified:
                    {
                        var conversation = await _store.GetConversationAsync(args.Id);

                        if (!ChatConversations.Any(x => x.ChatConversation.Id == args.Id))
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                ChatConversations.Insert(0, new ChatMenuItemControl(conversation));
                            });
                        }
                        else
                        {
                            var existingConversation = ChatConversations.First(x => x.ChatConversation.Id == args.Id);
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                bool wasSelected = SelectedItem == existingConversation;
                                var newConvo = new ChatMenuItemControl(conversation);

                                ChatConversations.Remove(existingConversation);
                                ChatConversations.Insert(0, newConvo);

                                if (wasSelected)
                                    SelectedItem = newConvo;
                            });
                        }
                        break;
                    }
                case ChatStoreChangedEventKind.ConversationDeleted:
                    {
                        if (ChatConversations.Any(x => x.ChatConversation.Id == args.Id))
                        {
                            var existingConversation = ChatConversations.First(x => x.ChatConversation.Id == args.Id);
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                bool wasSelected = SelectedItem == existingConversation;

                                ChatConversations.Remove(existingConversation);

                                if (wasSelected && ChatConversations.Count != 0)
                                    SelectedItem = ChatConversations[0];
                            });
                        }
                        break;
                    }
            }
        }
    }
}
