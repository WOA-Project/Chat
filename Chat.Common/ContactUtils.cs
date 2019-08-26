//using PhoneNumbers;
//using LibPhoneNumber.Contrib.PhoneNumberUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Sms;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Globalization.PhoneNumberFormatting;

namespace Chat.Common
{
    public class ContactUtils
    {
        public class ContactInformation
        {
            public string DisplayName { get; set; }
            public string ThumbnailPath { get; set; }
            public string PhoneNumberKind { get; set; }
        }

        public static async Task<Contact> GetMyself()
        {
            var store = await ContactManager.RequestStoreAsync();
            if (store != null)
            {
                var contact = await store.GetMeContactAsync();

                if (contact != null)
                    return contact;
            }

            Contact blankcontact = new Contact();
            blankcontact.Phones.Add(new ContactPhone() { Number = "Me", Kind = ContactPhoneKind.Other });
            return blankcontact;
        }

        private class PhoneNumberInfo2
        {
            public string Number { get; set; }
            public string CountryCode { get; set; }
        }

        private static PhoneNumberInfo2 GetPhoneNumberInformation(string phonenumber)
        {
            PhoneNumberInfo2 info = new PhoneNumberInfo2();
            /*PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber number;
            var supportedCodes = phoneUtil.GetSupportedRegions().ToArray();
            var result = phoneUtil.TryGetValidNumber(phonenumber, supportedCodes, out number);*/

            var countrycode = "";
            var nationalnumber = phonenumber;

            /*if (result)
            {
                countrycode = number.CountryCode.ToString();
                nationalnumber = number.NationalNumber.ToString();
            }*/

            info.Number = nationalnumber;
            info.CountryCode = countrycode;

            return info;
        }

        public static bool ArePhoneNumbersMostLikelyTheSame(string num1, string num2)
        {
            var formatter = new PhoneNumberFormatter();

            var fnum1 = formatter.FormatPartialString(num1);
            var fnum2 = formatter.FormatPartialString(num2);

            if (fnum1 == fnum2)
                return true;

            var inum1 = new PhoneNumberInfo(fnum1);
            var inum2 = new PhoneNumberInfo(fnum2);

            var match = inum1.CheckNumberMatch(inum2);

            if (match == PhoneNumberMatchResult.ExactMatch || match == PhoneNumberMatchResult.NationalSignificantNumberMatch || match == PhoneNumberMatchResult.ShortNationalSignificantNumberMatch)
            {
                return true;
            }

            var info = GetPhoneNumberInformation(num1);

            string number = info.Number;
            string countrycode = info.CountryCode;

            if (string.IsNullOrEmpty(countrycode))
            {
                if (num2.ToLower().Replace(" ", "").Replace("(", "").Replace(")", "") == number.ToLower().Replace(" ", "").Replace("(", "").Replace(")", ""))
                    return true;
            }
            else
            {
                var info2 = GetPhoneNumberInformation(num1);

                string number2 = info2.Number;
                string countrycode2 = info2.CountryCode;

                if (number == number2 && countrycode == countrycode2)
                {
                    return true;
                }
                else if (string.IsNullOrEmpty(countrycode2))
                {
                    if (num2.Replace(" ", "").Replace("(", "").Replace(")", "") == number.ToLower().Replace(" ", "").Replace("(", "").Replace(")", ""))
                        return true;
                }
                else if (number == number2)
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<Contact> BindPhoneNumberToGlobalContact(string phonenumber)
        {
            var store = await ContactManager.RequestStoreAsync();
            if (store != null)
            {
                var contacts = await store.FindContactsAsync();

                foreach (var contact in contacts)
                {
                    foreach (var num in contact.Phones)
                    {
                        if (ArePhoneNumbersMostLikelyTheSame(phonenumber, num.Number))
                            return contact;
                    }
                }
            }

            Contact blankcontact = new Contact();
            blankcontact.Phones.Add(new ContactPhone() { Number = phonenumber, Kind = ContactPhoneKind.Other });
            return blankcontact;
        }

        public async static Task<ContactInformation> FindContactInformationFromSmsMessage(ISmsMessageBase message)
        {
            string from = "";

            switch (message.MessageType)
            {
                case SmsMessageType.Text:
                    {
                        from = ((SmsTextMessage2)message).From;
                        break;
                    }
                case SmsMessageType.App:
                    {
                        from = ((SmsAppMessage)message).From;
                        break;
                    }
                case SmsMessageType.Broadcast:
                    {
                        break;
                    }
                case SmsMessageType.Status:
                    {
                        from = ((SmsStatusMessage)message).From;
                        break;
                    }
                case SmsMessageType.Voicemail:
                    {
                        break;
                    }
                case SmsMessageType.Wap:
                    {
                        from = ((SmsWapMessage)message).From;
                        break;
                    }
            }

            return await FindContactInformationFromSender(from);
        }

        public async static Task<ContactInformation> FindContactInformationFromSender(string from)
        {
            ContactInformation info = new ContactInformation() { DisplayName = from, PhoneNumberKind = "Unknown", ThumbnailPath = "" };

            try
            {
                var contact = await BindPhoneNumberToGlobalContact(from);
                info.DisplayName = contact.DisplayName;

                try
                {
                    info.PhoneNumberKind = contact.Phones.First(x => ArePhoneNumbersMostLikelyTheSame(x.Number, from)).Kind.ToString();
                }
                catch
                {

                }

                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

                StorageFile file = await storageFolder.CreateFileAsync(contact.Id + ".png", CreationCollisionOption.ReplaceExisting);

                using (var srcStream = await contact.SmallDisplayPicture.OpenReadAsync())
                using (var targetStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                using (var reader = new DataReader(srcStream.GetInputStreamAt(0)))
                {
                    var output = targetStream.GetOutputStreamAt(0);
                    await reader.LoadAsync((uint)srcStream.Size);
                    while (reader.UnconsumedBufferLength > 0)
                    {
                        uint dataToRead = reader.UnconsumedBufferLength > 64
                                            ? 64
                                            : reader.UnconsumedBufferLength;

                        IBuffer buffer = reader.ReadBuffer(dataToRead);
                        await output.WriteAsync(buffer);
                    }

                    await output.FlushAsync();

                    info.ThumbnailPath = file.Path;
                }
            }
            catch
            {

            }

            return info;
        }

        public async static void AssignAppToPhoneContacts()
        {
            try
            {
                var store = await ContactManager.RequestStoreAsync();

                var contacts = await store.FindContactsAsync();

                if (contacts != null)
                {
                    var phonecontacts = contacts.Where(x => x.Phones.Count != 0);
                    if (phonecontacts != null)
                    {
                        foreach (var phonecontact in phonecontacts)
                        {
                            ContactAnnotationStore annotationStore = await ContactManager.RequestAnnotationStoreAsync(ContactAnnotationStoreAccessType.AppAnnotationsReadWrite);

                            ContactAnnotationList annotationList;

                            IReadOnlyList<ContactAnnotationList> annotationLists = await annotationStore.FindAnnotationListsAsync();
                            if (0 == annotationLists.Count)
                                annotationList = await annotationStore.CreateAnnotationListAsync();
                            else
                                annotationList = annotationLists[0];

                            ContactAnnotation annotation = new ContactAnnotation();
                            annotation.ContactId = phonecontact.Id;
                            annotation.RemoteId = phonecontact.Id;

                            annotation.SupportedOperations = ContactAnnotationOperations.Message;

                            await annotationList.TrySaveAnnotationAsync(annotation);
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
