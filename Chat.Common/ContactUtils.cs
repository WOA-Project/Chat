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

        private static (string, string) GetPhoneNumberInformation(string phonenumber)
        {
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

            return (nationalnumber, countrycode);
        }

        public static bool ArePhoneNumbersMostLikelyTheSame(string num1, string num2)
        {
            (string number, string countrycode) = GetPhoneNumberInformation(num1);
            if (string.IsNullOrEmpty(countrycode))
            {
                if (num2.ToLower().Replace(" ", "") == number.ToLower().Replace(" ", ""))
                    return true;
            }
            else
            {
                (string number2, string countrycode2) = GetPhoneNumberInformation(num2);
                if (number == number2 && countrycode == countrycode2)
                {
                    return true;
                }
                else if (string.IsNullOrEmpty(countrycode2))
                {
                    if (num2.Replace(" ", "") == number.ToLower().Replace(" ", ""))
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

        public async static Task<ContactInformation> FindContactInformationFromSmsTextMessage(SmsTextMessage2 message)
        {
            ContactInformation info = new ContactInformation() { DisplayName = message.From, PhoneNumberKind = "Unknown", ThumbnailPath = "" };

            try
            {
                var contact = await BindPhoneNumberToGlobalContact(message.From);
                info.DisplayName = contact.DisplayName;

                try
                {
                    info.PhoneNumberKind = contact.Phones.First(x => ArePhoneNumbersMostLikelyTheSame(x.Number, message.From)).Kind.ToString();
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
