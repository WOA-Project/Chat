using System;
using System.Threading.Tasks;
using Windows.Devices.Sms;

namespace Chat.Common
{
    public class SmsUtils
    {
        public async static Task<bool> SendTextMessageAsync(SmsDevice2 device, string[] numbers, string textmessage)
        {
            bool returnresult = true;

            try
            {
                SmsTextMessage2 message = new SmsTextMessage2();
                message.Body = textmessage;

                foreach (var number in numbers)
                {
                    message.To = number.TrimStart().TrimEnd();

                    try
                    {
                        SmsSendMessageResult result = await device.SendMessageAndGetResultAsync(message);

                        if (!result.IsSuccessful)
                            returnresult = false;
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

        public async static Task<bool> SendTextMessageAsync(SmsDevice2 device, string number, string textmessage)
        {
            return await SendTextMessageAsync(device, new string[1] { number }, textmessage);
        }
    }
}
