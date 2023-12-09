using Chat.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Chat.ContentDialogs
{
    public sealed partial class AboutContentDialog : ContentDialog
    {
        public AboutViewModel ViewModel { get; } = new AboutViewModel();

        public AboutContentDialog()
        {
            InitializeComponent();
        }

        private ICommand _closeDialogCommand;
        public ICommand CloseDialogCommand
        {
            get
            {
                _closeDialogCommand ??= new RelayCommand(
                        Hide);
                return _closeDialogCommand;
            }
        }
    }
}
