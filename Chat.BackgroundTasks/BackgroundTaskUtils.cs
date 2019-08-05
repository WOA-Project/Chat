using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Devices.Sms;

namespace Chat.BackgroundTasks
{
    public sealed class BackgroundTaskUtils
    {
        public static void RegisterToastNotificationBackgroundTasks()
        {
            try
            {
                SmsFilterRule filter = new SmsFilterRule(SmsMessageType.Text);
                SmsFilterActionType actionType = SmsFilterActionType.Accept;
                SmsFilterRules filterRules = new SmsFilterRules(actionType);
                IList<SmsFilterRule> rules = filterRules.Rules;
                rules.Add(filter);
                SmsMessageReceivedTrigger trigger = new SmsMessageReceivedTrigger(filterRules);

                RegisterBackgroundTask<SmsBackgroundTask>(trigger);
            }
            catch
            {

            }

            try
            {
                RegisterBackgroundTask<SmsReplyBackgroundTask>(new ToastNotificationActionTrigger());
            }
            catch
            {

            }
        }

        internal static void RegisterBackgroundTask<T>(IBackgroundTrigger trigger)
        {
            string taskName = typeof(T).Name;

            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(taskName)))
                return;

            var builder = new BackgroundTaskBuilder()
            {
                Name = taskName,
                TaskEntryPoint = typeof(T).FullName
            };

            builder.SetTrigger(trigger);
            builder.Register();
        }
    }
}
