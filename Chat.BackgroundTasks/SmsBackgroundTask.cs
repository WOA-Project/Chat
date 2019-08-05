using Chat.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Sms;
using Windows.UI.Notifications;

namespace Chat.BackgroundTasks
{
    public sealed class SmsBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            DisplayToast(taskInstance).Wait();
        }

        private async Task DisplayToast(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                SmsMessageReceivedTriggerDetails smsDetails = taskInstance.TriggerDetails as SmsMessageReceivedTriggerDetails;
                SmsTextMessage2 smsTextMessage;

                string deviceid = "";

                if (smsDetails.MessageType == SmsMessageType.Text)
                {
                    smsTextMessage = smsDetails.TextMessage;
                    deviceid = smsTextMessage.DeviceId;

                    var information = await ContactUtils.FindContactInformationFromSmsTextMessage(smsTextMessage);

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
                                        Text = information.DisplayName,
                                        HintMaxLines = 1
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = smsTextMessage.Body
                                    }
                                },
                                AppLogoOverride = new ToastGenericAppLogo()
                                {
                                    Source = information.ThumbnailPath,
                                    HintCrop = ToastGenericAppLogoCrop.Circle
                                },
                                Attribution = new ToastGenericAttributionText()
                                {
                                    Text = information.PhoneNumberKind
                                }
                            }
                        },
                        Actions = new ToastActionsCustom()
                        {
                            Inputs =
                            {
                                new ToastTextBox("textBox")
                                {
                                    PlaceholderContent = "Send a message"
                                }
                            },
                            Buttons =
                            {
                                new ToastButton("Send", "action=reply" + "&from=" + smsTextMessage.From + "&deviceid=" + deviceid)
                                {
                                    ActivationType = ToastActivationType.Background,
                                    ImageUri = "Assets/ToastIcons/Send.png",
                                    TextBoxId = "textBox"
                                }
                            }
                        },
                        Launch = "action=openThread" + "&from=" + smsTextMessage.From + "&deviceid=" + deviceid,
                        Audio = new ToastAudio()
                        {
                            Src = new Uri("ms-winsoundevent:Notification.SMS")
                        }
                    };

                    var toastNotif = new ToastNotification(toastContent.GetXml());
                    ToastNotificationManager.CreateToastNotifier().Show(toastNotif);

                    try
                    {
                        BadgeHandler.IncreaseBadgeNumber();
                    }
                    catch
                    {

                    }
                }

                smsDetails.Accept();
            }
            catch
            {

            }
        }

    }
}
