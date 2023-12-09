using Chat.Common;
using Chat.Controls;
using Chat.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Chat;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Chat.Pages
{
    public sealed partial class ConversationPage : Page
    {
        public ConversationViewModel ViewModel { get; } = new ConversationViewModel("");
        internal string ConversationId;

        public ConversationPage()
        {
            InitializeComponent();

            MessageListView.Loaded += MessageListView_Loaded;
            MessageListView.SizeChanged += MessageListView_SizeChanged;
        }

        private void MessageListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (object item in MessageListView.Items)
            {
                ChatMessageViewControl control = item as ChatMessageViewControl;
                control.RefreshVisuals();
            }
        }

        private void MessageListView_Loaded(object sender, RoutedEventArgs e)
        {
            Border rootBorder = VisualTreeHelper.GetChild(MessageListView, 0) as Border;
            ScrollViewer scrollViewer = rootBorder.Child as ScrollViewer;
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            foreach (object item in MessageListView.Items)
            {
                ChatMessageViewControl control = item as ChatMessageViewControl;
                control.RefreshVisuals();
            }
        }

        private void CellularLineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CellularLineComboBox.SelectedItem != null)
            {
                SendButton.IsEnabled = !string.IsNullOrEmpty(ComposeTextBox.Text);
            }
        }

        private void ComposeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CellularLineComboBox.SelectedItem != null)
            {
                SendButton.IsEnabled = !string.IsNullOrEmpty(ComposeTextBox.Text);
            }
        }

        private ICommand _showAttachments;
        public ICommand ShowAttachments
        {
            get
            {
                _showAttachments ??= new RelayCommand(
                        () =>
                        {
                            FlyoutBase.ShowAttachedFlyout(AttachmentButton);
                        });
                return _showAttachments;
            }
        }

        private ICommand _startCall;
        public ICommand StartCall
        {
            get
            {
                _startCall ??= new RelayCommand(
                        async () =>
                        {
                            ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();
                            _ = await Launcher.LaunchUriAsync(new Uri("tel:" + (await store.GetConversationAsync(ConversationId)).Participants.First()));
                        });
                return _startCall;
            }
        }

        private ICommand _sendReply;
        public ICommand SendReply
        {
            get
            {
                _sendReply ??= new RelayCommand(
                        async () =>
                        {
                            SendButton.IsEnabled = false;
                            ComposeTextBox.IsEnabled = false;
                            Windows.Devices.Sms.SmsDevice2 smsDevice = ViewModel.SelectedLine.device;

                            try
                            {
                                ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();
                                bool result = await SmsUtils.SendTextMessageAsync(smsDevice, (await store.GetConversationAsync(ConversationId)).Participants.First(), ComposeTextBox.Text);
                                if (!result)
                                {
                                    _ = await new MessageDialog("We could not send one or some messages.", "Something went wrong").ShowAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                _ = await new MessageDialog($"We could not send one or some messages.\n{ex}", "Something went wrong").ShowAsync();
                            }

                            SendButton.IsEnabled = true;
                            ComposeTextBox.IsEnabled = true;
                            ComposeTextBox.Text = "";
                        });
                return _sendReply;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e != null)
            {
                if (e.Parameter is string args)
                {
                    ConversationId = args;
                    ViewModel.Initialize(ConversationId);
                }
            }
        }
    }
}
