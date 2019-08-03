using Chat.Common;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Sms;
using Windows.UI.Notifications;

namespace Chat.BackgroundTasks
{
    public sealed class SmsReplyBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            HandleTaskActions(taskInstance).Wait();
        }

        private async Task HandleTaskActions(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                if (taskInstance.TriggerDetails is ToastNotificationActionTriggerDetail)
                {
                    try
                    {
                        BadgeHandler.DecreaseBadgeNumber();
                    }
                    catch
                    {

                    }

                    var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;

                    string arguments = details.Argument;

                    string action = details.Argument.Split('&').First(x => x.ToLower().StartsWith("action=")).Split('=')[1];
                    string from = details.Argument.Split('&').First(x => x.ToLower().StartsWith("from=")).Split('=')[1];
                    string deviceid = details.Argument.Split('&').First(x => x.ToLower().StartsWith("deviceid=")).Split('=')[1];

                    switch (action.ToLower())
                    {
                        case "reply":
                            {
                                try
                                {
                                    string messagetosend = (string)details.UserInput["textBox"];
                                    SmsDevice2 smsDevice = SmsDevice2.FromId(deviceid);
                                    await SmsUtils.SendTextMessageAsync(smsDevice, from, messagetosend);
                                }
                                catch
                                {

                                }

                                break;
                            }
                        case "openthread":
                            {
                                break;
                            }
                    }
                }
            }
            catch
            {
                
            }
        }
    }
}
