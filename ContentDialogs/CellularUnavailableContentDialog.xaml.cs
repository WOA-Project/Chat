using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Chat.ContentDialogs
{
    public sealed partial class CellularUnavailableContentDialog : ContentDialog
    {
        public CellularUnavailableContentDialog()
        {
            this.InitializeComponent();
        }

        private ICommand _closeDialogCommand;
        public ICommand CloseDialogCommand
        {
            get
            {
                if (_closeDialogCommand == null)
                {
                    _closeDialogCommand = new RelayCommand(
                        () =>
                        {
                            Hide();
                        });
                }
                return _closeDialogCommand;
            }
        }
    }
}
