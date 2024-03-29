﻿using System;
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
                SmsFilterActionType actionType = SmsFilterActionType.Accept;
                SmsFilterRules filterRules = new SmsFilterRules(actionType);
                IList<SmsFilterRule> rules = filterRules.Rules;

                SmsFilterRule filter = new SmsFilterRule(SmsMessageType.Text);
                rules.Add(filter);
                filter = new SmsFilterRule(SmsMessageType.Wap);
                rules.Add(filter);
                filter = new SmsFilterRule(SmsMessageType.Status);
                rules.Add(filter);
                filter = new SmsFilterRule(SmsMessageType.App);
                rules.Add(filter);

                SmsMessageReceivedTrigger trigger = new SmsMessageReceivedTrigger(filterRules);

                RegisterBackgroundTask<SmsBackgroundTask>(trigger);
            }
            catch
            {

            }

            // Was an experiment, causes double notifications if registered by design
            /*try
            {
                var transports = await ChatMessageManager.GetTransportsAsync();
                foreach (var transport in transports)
                {
                    if (!transport.IsAppSetAsNotificationProvider)// && transport.TransportKind == ChatMessageTransportKind.Text)
                    {
                        await transport.RequestSetAsNotificationProviderAsync();
                    }
                }
            }
            catch
            {

            }

            try
            {
                RegisterBackgroundTask<ChatMessageNotificationBackgroundTask>(new ChatMessageNotificationTrigger());
            }
            catch
            {

            }*/

            try
            {
                RegisterBackgroundTask<SmsReplyBackgroundTask>(new ToastNotificationActionTrigger());
            }
            catch
            {

            }
        }

        public static void UnRegisterToastNotificationBackgroundTasks()
        {
            try
            {
                UnRegisterBackgroundTask<SmsBackgroundTask>();
            }
            catch
            {

            }

            try
            {
                UnRegisterBackgroundTask<ChatMessageNotificationBackgroundTask>();
            }
            catch
            {

            }

            try
            {
                UnRegisterBackgroundTask<SmsReplyBackgroundTask>();
            }
            catch
            {

            }
        }

        internal static void UnRegisterBackgroundTask<T>()
        {
            string taskName = typeof(T).Name;

            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(taskName)))
            {
                KeyValuePair<Guid, IBackgroundTaskRegistration> task = BackgroundTaskRegistration.AllTasks.First(i => i.Value.Name.Equals(taskName));
                task.Value.Unregister(true);
            }
        }

        internal static void RegisterBackgroundTask<T>(IBackgroundTrigger trigger)
        {
            string taskName = typeof(T).Name;

            if (BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals(taskName)))
            {
                return;
            }

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder()
            {
                Name = taskName,
                TaskEntryPoint = typeof(T).FullName
            };

            builder.SetTrigger(trigger);
            _ = builder.Register();
        }
    }
}
