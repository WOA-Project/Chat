using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.Devices.Sms;

namespace Chat.Common
{
    public class SmsUtils
    {
        public async static Task<bool> SendTextMessageAsync(SmsDevice2 device, string[] numbers, string textmessage, string transportId = "0")
        {
            bool returnresult = true;
            ChatMessageStore store = await ChatMessageManager.RequestStoreAsync();

            if (string.IsNullOrEmpty(transportId) || await ChatMessageManager.GetTransportAsync(transportId) == null)
            {
                var transports = await ChatMessageManager.GetTransportsAsync();
                if (transports.Count != 0)
                {
                    var transport = transports[0];
                    transportId = transport.TransportId;
                }
                else
                {
                    transportId = await ChatMessageManager.RegisterTransportAsync();
                }
            }
            try
            {
                SmsTextMessage2 message = new SmsTextMessage2();
                message.Body = textmessage;

                foreach (var number in numbers)
                {
                    var num = number.Trim();
                    message.To = num;

                    try
                    {
                        SmsSendMessageResult result = await device.SendMessageAndGetResultAsync(message);
                        var offset = new DateTimeOffset(DateTime.Now);

                        returnresult &= result.IsSuccessful;

                        try
                        {
                            ChatMessage msg = new ChatMessage();

                            msg.Body = textmessage;
                            msg.Recipients.Add(num);

                            msg.LocalTimestamp = offset;
                            msg.NetworkTimestamp = offset;

                            msg.IsIncoming = false;
                            msg.TransportId = transportId;

                            msg.MessageOperatorKind = ChatMessageOperatorKind.Sms;
                            msg.Status = result.IsSuccessful ? ChatMessageStatus.Sent : ChatMessageStatus.SendFailed;

                            await store.SaveMessageAsync(msg);
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {
                        returnresult = false;
                    }
                }
            }
            catch
            {
                returnresult = false;
            }

            return returnresult;
        }

        public async static Task<bool> SendTextMessageAsync(SmsDevice2 device, string number, string textmessage, string transportId = "0")
        {
            return await SendTextMessageAsync(device, new string[1] { number }, textmessage, transportId);
        }
    }
}
