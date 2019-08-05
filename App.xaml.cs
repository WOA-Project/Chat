using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace Chat
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Shell rootShell = Window.Current.Content as Shell;

            if (rootShell == null)
            {
                rootShell = new Shell();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                rootShell.HandleArguments(e);

                Window.Current.Content = rootShell;
            }

            if (e.PrelaunchActivated == false)
            {
                rootShell.HandleArguments(e);
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            if (e.Kind == ActivationKind.Protocol)
            {
                Shell rootShell = Window.Current.Content as Shell;
                if (rootShell == null)
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
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
