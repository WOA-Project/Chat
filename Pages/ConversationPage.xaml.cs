using Chat.Common;
using Chat.Controls;
using Chat.Helpers;
using Chat.ViewModels;
using GalaSoft.MvvmLight.Command;
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
            this.InitializeComponent();

            MessageListView.Loaded += MessageListView_Loaded;
            MessageListView.SizeChanged += MessageListView_SizeChanged;
        }

        private void MessageListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var item in MessageListView.Items)
            {
                var control = item as ChatMessageViewControl;
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
            foreach (var item in MessageListView.Items)
            {
                var control = item as ChatMessageViewControl;
                control.RefreshVisuals();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private void CellularLineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CellularLineComboBox.SelectedItem != null)
            {
                SendButton.IsEnabled = !string.IsNullOrEmpty(ComposeTextBox.Text);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
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
                if (_showAttachments == null)
                {
                    _showAttachments = new RelayCommand(
                        () =>
                        {
                            FlyoutBase.ShowAttachedFlyout((FrameworkElement)AttachmentButton);
                        });
                }
                return _showAttachments;
            }
        }

        private ICommand _startCall;
        public ICommand StartCall
        {
            get
            {
                if (_startCall == null)
                {
                    _startCall = new RelayCommand(
                        async () =>
                        {
                            var store = await ChatMessageManager.RequestStoreAsync();
                            await Launcher.LaunchUriAsync(new Uri("tel:" + (await store.GetConversationAsync(ConversationId)).Participants.First()));
                        });
                }
                return _startCall;
            }
        }

        private ICommand _sendReply;
        public ICommand SendReply
        {
            get
            {
                if (_sendReply == null)
                {
                    _sendReply = new RelayCommand(
                        async () =>
                        {
                            SendButton.IsEnabled = false;
                            ComposeTextBox.IsEnabled = false;
                            var smsDevice = ViewModel.SelectedLine.device;

                            try
                            {
                                var store = await ChatMessageManager.RequestStoreAsync();
                                var result = await SmsUtils.SendTextMessageAsync(smsDevice, (await store.GetConversationAsync(ConversationId)).Participants.First(), ComposeTextBox.Text);
                                if (!result)
                                    await new MessageDialog("We could not send one or some messages.", "Something went wrong").ShowAsync();
                            }
                            catch (Exception ex)
                            {
                                await new MessageDialog($"We could not send one or some messages.\n{ex}", "Something went wrong").ShowAsync();
                            }

                            SendButton.IsEnabled = true;
                            ComposeTextBox.IsEnabled = true;
                            ComposeTextBox.Text = "";
                        });
                }
                return _sendReply;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e != null)
            {
                var args = e.Parameter as string;
                if (args != null)
                {
                    ConversationId = args;
                    ViewModel.Initialize(ConversationId);
                }
            }
        }
    }
}
