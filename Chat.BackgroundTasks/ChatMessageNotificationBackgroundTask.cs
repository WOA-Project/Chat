using Chat.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Chat;
using Windows.Devices.Sms;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace Chat.BackgroundTasks
{
    public sealed class ChatMessageNotificationBackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Process(taskInstance).Wait();
        }

        private async Task Process(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                ChatMessageNotificationTriggerDetails triggerDetails = (ChatMessageNotificationTriggerDetails)taskInstance.TriggerDetails;
                if (triggerDetails.ShouldDisplayToast)
                {
                    await DisplayToast(triggerDetails.ChatMessage);
                }
            }
            catch
            {

            }
        }

        private async Task<string> SaveFile(IRandomAccessStreamReference stream, string filename)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            StorageFile file = await storageFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            using (IRandomAccessStreamWithContentType srcStream = await stream.OpenReadAsync())
            using (IRandomAccessStream targetStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            using (DataReader reader = new DataReader(srcStream.GetInputStreamAt(0)))
            {
                IOutputStream output = targetStream.GetOutputStreamAt(0);
                _ = await reader.LoadAsync((uint)srcStream.Size);
                while (reader.UnconsumedBufferLength > 0)
                {
                    uint dataToRead = reader.UnconsumedBufferLength > 64
                                        ? 64
                                        : reader.UnconsumedBufferLength;

                    IBuffer buffer = reader.ReadBuffer(dataToRead);
                    _ = await output.WriteAsync(buffer);
                }

                _ = await output.FlushAsync();

                return file.Path;
            }
        }

        private async Task DisplayToast(ChatMessage message)
        {
            ContactUtils.ContactInformation information = await ContactUtils.FindContactInformationFromSender(message.From);
            string thumbnailpath = "";
            string text = "";

            string deviceid = SmsDevice2.GetDefault().DeviceId;

            foreach (ChatMessageAttachment attachment in message.Attachments)
            {
                try
                {
                    //extra += " " + attachment.MimeType;

                    if (attachment.MimeType == "application/smil")
                    {

                    }

                    if (attachment.MimeType == "text/vcard")
                    {
                        text += "Contact content in this message. ";
                    }

                    if (attachment.MimeType.StartsWith("image/"))
                    {
                        text += "Image content in this message. ";
                        string imageextension = attachment.MimeType.Split('/').Last();
                        thumbnailpath = await SaveFile(attachment.DataStreamReference, "messagepicture." + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "." + imageextension);
                    }

                    if (attachment.MimeType.StartsWith("audio/"))
                    {
                        text += "Audio content in this message. ";
                        string audioextension = attachment.MimeType.Split('/').Last();
                    }
                }
                catch
                {
                    //extra += " oops";
                }
            }

            ToastContent toastContent = new ToastContent()
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
                                        Text = string.IsNullOrEmpty(message.Body) ? text : message.Body
                                    }
                                },
                        HeroImage = new ToastGenericHeroImage()
                        {
                            Source = thumbnailpath
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = information.ThumbnailPath,
                            HintCrop = ToastGenericAppLogoCrop.Circle
                        },
                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = information.PhoneNumberKind //+ " " + extra
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
                                new ToastButton("Send", "action=reply" + "&from=" + message.From + "&deviceid=" + deviceid)
                                {
                                    ActivationType = ToastActivationType.Background,
                                    ImageUri = "Assets/ToastIcons/Send.png",
                                    TextBoxId = "textBox"
                                }
                            }
                },
                Launch = "action=openThread" + "&from=" + message.From + "&deviceid=" + deviceid,
                Audio = new ToastAudio()
                {
                    Src = new Uri("ms-winsoundevent:Notification.SMS")
                }
            };

            ToastNotification toastNotif = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }
    }
}
