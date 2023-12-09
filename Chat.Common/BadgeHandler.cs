namespace Chat.Common
{
    public class BadgeHandler
    {
        public static void IncreaseBadgeNumber()
        {
            return;
            /*ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (!localSettings.Values.ContainsKey("UnreadCount"))
            {
                localSettings.Values["UnreadCount"] = 0;
            }

            int count = (int)localSettings.Values["UnreadCount"];

            if (count < 0)
            {
                localSettings.Values["UnreadCount"] = 0;
                count = 0;
            }

            count++;
            localSettings.Values["UnreadCount"] = count;

            setBadgeNumber(count);*/
        }

        public static void DecreaseBadgeNumber()
        {
            return;
            /*ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            if (!localSettings.Values.ContainsKey("UnreadCount"))
            {
                localSettings.Values["UnreadCount"] = 0;
            }

            int count = (int)localSettings.Values["UnreadCount"];

            if (count < 0)
            {
                localSettings.Values["UnreadCount"] = 0;
                count = 0;
            }

            if (count != 0)
            {
                count--;
            }

            localSettings.Values["UnreadCount"] = count;

            setBadgeNumber(count);*/
        }

        /*private static void setBadgeNumber(int num)
        {
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", num.ToString());

            BadgeNotification badge = new BadgeNotification(badgeXml);

            BadgeUpdater badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();

            badgeUpdater.Update(badge);
        }*/
    }
}
