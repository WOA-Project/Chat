using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace Chat.BackgroundTasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            /*
             * Windows has issues with our SMS background tasks when the app gets updated
             * The background tasks will stay registered
             * But the trigger will be lost, so they won't run ever again
             * So we re-register the background tasks entirely to fix that issue
             */

            BackgroundTaskUtils.UnRegisterToastNotificationBackgroundTasks();
            BackgroundTaskUtils.RegisterToastNotificationBackgroundTasks();

            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Welcome back!"
                            },
                            new AdaptiveText()
                            {
                                Text = "Chat has been updated."
                            }
                        }
                    }
                }
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml());

            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }
    }
}
