using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Chat.ContentDialogs
{
    public sealed partial class CellularUnavailableContentDialog : ContentDialog
    {
        public CellularUnavailableContentDialog()
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
