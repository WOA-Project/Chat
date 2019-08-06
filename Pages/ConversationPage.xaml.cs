using Chat.Common;
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
using Windows.UI.Xaml.Navigation;

namespace Chat.Pages
{
    public sealed partial class ConversationPage : Page
    {
        public ConversationViewModel ViewModel { get; } = new ConversationViewModel("");
        public string ConversationId;

        public ConversationPage()
        {
            this.InitializeComponent();
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
                            try
                            {
                                SendButton.IsEnabled = false;
                                var smsDevice = ViewModel.SelectedLine.device;

                                var store = await ChatMessageManager.RequestStoreAsync();
                                var result = await SmsUtils.SendTextMessageAsync(smsDevice, (await store.GetConversationAsync(ConversationId)).Participants.First(), ComposeTextBox.Text);
                                if (!result)
                                    await new MessageDialog("We could not send one or some messages.", "Something went wrong").ShowAsync();

                                SendButton.IsEnabled = true;
                                ComposeTextBox.Text = "";
                            }
                            catch (Exception ex)
                            {
                                SendButton.IsEnabled = true;
                                await new MessageDialog(ex.Message + " - " + ex.StackTrace).ShowAsync();
                            }
                        });
                }
                return _sendReply;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var args = e.Parameter as string;
            if (args != null)
            {
                ConversationId = args;
                ViewModel.Initialize(ConversationId);
            }
        }
    }
}
