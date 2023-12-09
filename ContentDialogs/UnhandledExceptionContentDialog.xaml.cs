using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Chat.ContentDialogs
{
    public sealed partial class UnhandledExceptionContentDialog : ContentDialog
    {
        public UnhandledExceptionContentDialog(string Description)
        {
            InitializeComponent();
            UnhandledExceptionDescription.Text = Description;
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
