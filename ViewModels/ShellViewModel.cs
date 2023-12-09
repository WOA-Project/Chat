using Chat.BackgroundTasks;
using Chat.Common;
using Chat.ContentDialogs;
using Chat.Controls;
using Chat.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.Sms;
using Windows.UI.Core;

namespace Chat.ViewModels
{
    public class ShellViewModel : Observable
    {
        // Properties
        private ObservableCollection<ChatMenuItemControl> _chatConversations;
        public ObservableCollection<ChatMenuItemControl> ChatConversations
        {
            get => _chatConversations;
            internal set => Set(ref _chatConversations, value);
        }

        private ChatMenuItemControl _selectedItem;
        public ChatMenuItemControl SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
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
            {
                SelectedItem = ChatConversations[0];
            }

            if (!await PerformMandatoryChecks())
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    _ = await new CellularUnavailableContentDialog().ShowAsync();
                });
            }

            _store.ChangeTracker.Enable();
            _store.StoreChanged += Store_StoreChanged;
        }

        private static async Task<bool> PerformMandatoryChecks()
        {
            bool available = false;
            try
            {
                DeviceInformationCollection smsDevices = await DeviceInformation.FindAllAsync(SmsDevice2.GetDeviceSelector(), null);
                foreach (DeviceInformation smsDevice in smsDevices)
                {
                    try
                    {
                        SmsDevice2 dev = SmsDevice2.FromId(smsDevice.Id);
                        switch (dev.DeviceStatus)
                        {
                            case SmsDeviceStatus.Ready:
                                {
                                    return true;
                                }
                        }
                    }
                    catch
                    {

                    }
                }
            }
            catch
            {
                available = false;
            }
            return available;
        }

        // Methods
        private async Task<ObservableCollection<ChatMenuItemControl>> GetChats()
        {
            ObservableCollection<ChatMenuItemControl> collection = [];

            ChatConversationReader reader = _store.GetConversationReader();
            System.Collections.Generic.IReadOnlyList<ChatConversation> convos = await reader.ReadBatchAsync();

            foreach (ChatConversation convo in convos)
            {
                collection.Add(new ChatMenuItemControl(convo.Id));
            }

            return collection;
        }

        private async void Store_StoreChanged(ChatMessageStore sender, ChatMessageStoreChangedEventArgs args)
        {
            switch (args.Kind)
            {
                case ChatStoreChangedEventKind.ConversationModified:
                    {
                        ChatConversation conversation = await _store.GetConversationAsync(args.Id);

                        if (conversation == null)
                        {
                            if (ChatConversations.Any(x => x.ConversationId == args.Id))
                            {
                                ChatMenuItemControl existingConversation = ChatConversations.First(x => x.ConversationId == args.Id);
                                bool wasSelected = SelectedItem == existingConversation;

                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                () =>
                                {
                                    _ = ChatConversations.Remove(existingConversation);

                                    if (wasSelected && ChatConversations.Count != 0)
                                    {
                                        SelectedItem = ChatConversations[0];
                                    }
                                });
                            }
                            break;
                        }

                        if (!ChatConversations.Any(x => x.ConversationId == args.Id))
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                ChatConversations.Insert(0, new ChatMenuItemControl(conversation.Id));
                                if (!ChatConversations.Contains(SelectedItem))
                                {
                                    SelectedItem = ChatConversations[0];
                                }
                            });
                        }
                        /*else
                        {
                            var existingConversation = ChatConversations.First(x => x.ConversationId == args.Id);
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                bool wasSelected = SelectedItem == existingConversation;
                                var newConvo = new ChatMenuItemControl(conversation.Id);

                                ChatConversations.Remove(existingConversation);
                                ChatConversations.Insert(0, newConvo);

                                if (wasSelected)
                                    SelectedItem = newConvo;
                            });
                        }*/
                        break;
                    }
                case ChatStoreChangedEventKind.ConversationDeleted:
                    {
                        if (ChatConversations.Any(x => x.ConversationId == args.Id))
                        {
                            ChatMenuItemControl existingConversation = ChatConversations.First(x => x.ConversationId == args.Id);
                            bool wasSelected = SelectedItem == existingConversation;

                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                _ = ChatConversations.Remove(existingConversation);

                                if (wasSelected && ChatConversations.Count != 0)
                                {
                                    SelectedItem = ChatConversations[0];
                                }
                            });
                        }
                        break;
                    }
            }
        }
    }
}
