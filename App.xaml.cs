using Chat.ContentDialogs;
using System;
using System.Globalization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace Chat
{
    public sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            UnhandledException += App_UnhandledException;
        }

        private async void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            string ExceptionDesc = e.Exception.Message + "\nHRESULT: 0x" + e.Exception.HResult.ToString("X4", new CultureInfo("en-US")) + "\n" + e.Exception.StackTrace + "\n" + e.Exception.Source;
            if (e.Exception.InnerException != null)
            {
                ExceptionDesc += "\n\n" + e.Exception.InnerException.Message + "\nHRESULT: 0x" + e.Exception.InnerException.HResult.ToString("X4", new CultureInfo("en-US")) + "\n" + e.Exception.InnerException.StackTrace + "\n" + e.Exception.InnerException.Source;
            }
            else
            {
                ExceptionDesc += "\n\nNo inner exception was thrown";
            }

            _ = await new UnhandledExceptionContentDialog(ExceptionDesc).ShowAsync();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (Window.Current.Content is not Shell rootShell)
            {
                rootShell = new Shell();

                if (e != null && e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                if (e != null)
                {
                    rootShell.HandleArguments(e);
                }

                Window.Current.Content = rootShell;
            }

            if (e != null && e.PrelaunchActivated == false)
            {
                rootShell.HandleArguments(e);
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            if ((e != null && e.Kind == ActivationKind.Protocol) || (e != null && e.Kind == ActivationKind.ToastNotification))
            {
                if (Window.Current.Content is not Shell rootShell)
                {
                    rootShell = new Shell();

                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }

                    Window.Current.Content = rootShell;
                }

                rootShell.HandleArguments(e);

                Window.Current.Activate();
            }
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
