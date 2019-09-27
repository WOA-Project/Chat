using Chat.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Sms;
using Windows.Storage;
using Windows.Storage.Streams;
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

                string deviceid = "";
                string body = "";
                string from = "";
                ContactUtils.ContactInformation information = new ContactUtils.ContactInformation();

                switch (smsDetails.MessageType)
                {
                    case SmsMessageType.Text:
                        {
                            SmsTextMessage2 smsTextMessage = smsDetails.TextMessage;
                            body = smsTextMessage.Body;
                            deviceid = smsTextMessage.DeviceId;
                            from = smsTextMessage.From;
                            information = await ContactUtils.FindContactInformationFromSmsMessage(smsTextMessage);
                            break;
                        }
                    /*case SmsMessageType.Wap:
                        {
                            SmsWapMessage smsWapMessage = smsDetails.WapMessage;
                            body = "[DEBUG - Report if seen] " + smsWapMessage.ContentType + " - " + "Wap";
                            deviceid = smsWapMessage.DeviceId;
                            from = smsWapMessage.From;
                            information = await ContactUtils.FindContactInformationFromSmsMessage(smsWapMessage);
                            break;
                        }
                    case SmsMessageType.App:
                        {
                            SmsAppMessage smsAppMessage = smsDetails.AppMessage;
                            body = "[DEBUG - Report if seen] " + smsAppMessage.Body + " - RAW: " + BitConverter.ToString(smsAppMessage.BinaryBody.ToArray()) + " - " + "App";
                            deviceid = smsAppMessage.DeviceId;
                            from = smsAppMessage.From;
                            information = await ContactUtils.FindContactInformationFromSmsMessage(smsAppMessage);
                            break;
                        }
                    case SmsMessageType.Status:
                        {
                            SmsStatusMessage smsStatusMessage = smsDetails.StatusMessage;
                            body = "[DEBUG - Report if seen] " + smsStatusMessage.Body + " - " + smsStatusMessage.Status.ToString() + " - " + "Status";
                            deviceid = smsStatusMessage.DeviceId;
                            from = smsStatusMessage.From;
                            information = await ContactUtils.FindContactInformationFromSmsMessage(smsStatusMessage);
                            break;
                        }*/
                    default:
                        return;
                }

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
                                        Text = body
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
                                    PlaceholderContent = "Type a message"
                                }
                            },
                        Buttons =
                            {
                                new ToastButton("Send", $"action=reply&from={from}&deviceid={deviceid}")
                                {
                                    ActivationType = ToastActivationType.Background,
                                    ImageUri = "Assets/ToastIcons/Send.png",
                                    TextBoxId = "textBox"
                                }
                            }
                    },
                    Launch = $"action=openThread&from={from}&deviceid={deviceid}",
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

                smsDetails.Accept();
            }
            catch
            {

            }
        }
    }
}
