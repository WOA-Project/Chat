//using PhoneNumbers;
//using LibPhoneNumber.Contrib.PhoneNumberUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Sms;
using Windows.Globalization.PhoneNumberFormatting;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Chat.Common
{
    public class ContactUtils
    {
        public class ContactInformation
        {
            public string DisplayName
            {
                get; set;
            }
            public string ThumbnailPath
            {
                get; set;
            }
            public string PhoneNumberKind
            {
                get; set;
            }
        }

        public static async Task<Contact> GetMyself()
        {
            ContactStore store = await ContactManager.RequestStoreAsync();
            if (store != null)
            {
                Contact contact = await store.GetMeContactAsync();

                if (contact != null)
                {
                    return contact;
                }
            }

            Contact blankcontact = new Contact();
            blankcontact.Phones.Add(new ContactPhone() { Number = "Me", Kind = ContactPhoneKind.Other });
            return blankcontact;
        }

        private class PhoneNumberInfo2
        {
            public string Number
            {
                get; set;
            }
            public string CountryCode
            {
                get; set;
            }
        }

        private static PhoneNumberInfo2 GetPhoneNumberInformation(string phonenumber)
        {
            PhoneNumberInfo2 info = new PhoneNumberInfo2();
            /*PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber number;
            var supportedCodes = phoneUtil.GetSupportedRegions().ToArray();
            var result = phoneUtil.TryGetValidNumber(phonenumber, supportedCodes, out number);*/

            string countrycode = "";
            string nationalnumber = phonenumber;

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
            PhoneNumberFormatter formatter = new PhoneNumberFormatter();

            string fnum1 = formatter.FormatPartialString(num1);
            string fnum2 = formatter.FormatPartialString(num2);

            if (fnum1 == fnum2)
            {
                return true;
            }

            PhoneNumberInfo inum1 = new PhoneNumberInfo(fnum1);
            PhoneNumberInfo inum2 = new PhoneNumberInfo(fnum2);

            PhoneNumberMatchResult match = inum1.CheckNumberMatch(inum2);

            if (match == PhoneNumberMatchResult.ExactMatch || match == PhoneNumberMatchResult.NationalSignificantNumberMatch || match == PhoneNumberMatchResult.ShortNationalSignificantNumberMatch)
            {
                return true;
            }

            PhoneNumberInfo2 info = GetPhoneNumberInformation(num1);

            string number = info.Number;
            string countrycode = info.CountryCode;

            if (string.IsNullOrEmpty(countrycode))
            {
                if (num2.ToLower().Replace(" ", "").Replace("(", "").Replace(")", "") == number.ToLower().Replace(" ", "").Replace("(", "").Replace(")", ""))
                {
                    return true;
                }
            }
            else
            {
                PhoneNumberInfo2 info2 = GetPhoneNumberInformation(num1);

                string number2 = info2.Number;
                string countrycode2 = info2.CountryCode;

                if (number == number2 && countrycode == countrycode2)
                {
                    return true;
                }
                else if (string.IsNullOrEmpty(countrycode2))
                {
                    if (num2.Replace(" ", "").Replace("(", "").Replace(")", "") == number.ToLower().Replace(" ", "").Replace("(", "").Replace(")", ""))
                    {
                        return true;
                    }
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
            try
            {
                ContactStore store = await ContactManager.RequestStoreAsync();
                if (store != null)
                {
                    IReadOnlyList<Contact> contacts = await store.FindContactsAsync();

                    foreach (Contact contact in contacts)
                    {
                        foreach (ContactPhone num in contact.Phones)
                        {
                            if (ArePhoneNumbersMostLikelyTheSame(phonenumber, num.Number))
                            {
                                return contact;
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            Contact blankcontact = new Contact();
            blankcontact.Phones.Add(new ContactPhone() { Number = phonenumber, Kind = ContactPhoneKind.Other });
            return blankcontact;
        }

        public static async Task<ContactInformation> FindContactInformationFromSmsMessage(ISmsMessageBase message)
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

        public static async Task<ContactInformation> FindContactInformationFromSender(string from)
        {
            ContactInformation info = new ContactInformation() { DisplayName = from, PhoneNumberKind = "Unknown", ThumbnailPath = "" };

            try
            {
                Contact contact = await BindPhoneNumberToGlobalContact(from);
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

                using (IRandomAccessStreamWithContentType srcStream = await contact.SmallDisplayPicture.OpenReadAsync())
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

                    info.ThumbnailPath = file.Path;
                }
            }
            catch
            {

            }

            return info;
        }

        public static async void AssignAppToPhoneContacts()
        {
            try
            {
                ContactStore store = await ContactManager.RequestStoreAsync();

                IReadOnlyList<Contact> contacts = await store.FindContactsAsync();

                if (contacts != null)
                {
                    IEnumerable<Contact> phonecontacts = contacts.Where(x => x.Phones.Count != 0);
                    if (phonecontacts != null)
                    {
                        foreach (Contact phonecontact in phonecontacts)
                        {
                            ContactAnnotationStore annotationStore = await ContactManager.RequestAnnotationStoreAsync(ContactAnnotationStoreAccessType.AppAnnotationsReadWrite);

                            ContactAnnotationList annotationList;

                            IReadOnlyList<ContactAnnotationList> annotationLists = await annotationStore.FindAnnotationListsAsync();
                            annotationList = 0 == annotationLists.Count ? await annotationStore.CreateAnnotationListAsync() : annotationLists[0];

                            ContactAnnotation annotation = new ContactAnnotation
                            {
                                ContactId = phonecontact.Id,
                                RemoteId = phonecontact.Id,

                                SupportedOperations = ContactAnnotationOperations.Message
                            };

                            _ = await annotationList.TrySaveAnnotationAsync(annotation);
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
