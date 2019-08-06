using PhoneNumbers;

// From https://raw.githubusercontent.com/Smartrak/Smartrak.Library/master/LibPhoneNumber.Contrib/PhoneNumberUtil/PhoneNumberUtilExtensions.cs

namespace LibPhoneNumber.Contrib.PhoneNumberUtil
{
    /// <summary>
    /// Extension methods for PhoneNumbers.PhoneNumberUtil
    /// </summary>
    public static class PhoneUtilExtensions
    {
        /// <summary>
        /// Tries get the PhoneNumber object if the phonenumber is a valid phoneNumber for the regions specified
        /// </summary>
        /// <param name="phoneUtil"></param>
        /// <param name="numberString"></param>
        /// <param name="regionCodes">The region code for global networks</param>
        /// <param name="phoneNumber">PhoneNumber object</param>
        /// <returns>True if successful; else false</returns>
        public static bool TryGetValidMobileNumber(this PhoneNumbers.PhoneNumberUtil phoneUtil, string numberString, string[] regionCodes, out PhoneNumbers.PhoneNumber phoneNumber)
        {
            phoneNumber = null;

            var number = phoneUtil.GetValidNumber(numberString, regionCodes);

            if (number == null)
                return false;

            return phoneUtil.GetNumberType(number) == PhoneNumberType.MOBILE;
        }


        /// <summary>
        /// Returns the PhoneNumbers.PhoneNumber object of the phonenumber
        /// </summary>
        /// <param name="phoneUtil">PhoneUtil instance</param>
        /// <param name="numberString">Phonenumber</param>
        /// <param name="regionCodes">The region code for global networks</param>
        /// <returns>Null if it was invalid phonenumber</returns>
        public static PhoneNumbers.PhoneNumber GetValidMobileNumber(this PhoneNumbers.PhoneNumberUtil phoneUtil, string numberString, string[] regionCodes)
        {
            var number = phoneUtil.GetValidNumber(numberString, regionCodes);

            if (number == null)
                return null;

            return phoneUtil.GetNumberType(number) == PhoneNumberType.MOBILE
                ? number
                : null;
        }

        /// <summary>
        /// Tries get a valid number for the regions passed in
        /// </summary>
        /// <param name="phoneUtil">PhoneUtil instance</param>
        /// <param name="numberString">Phonenumber</param>
        /// <param name="regionCodes">The region code for global networks</param>
        /// <param name="phoneNumber">PhoneNumbers.PhoneNumber object of the number passed in</param>
        /// <returns>True if successful; else false</returns>
        public static bool TryGetValidNumber(this PhoneNumbers.PhoneNumberUtil phoneUtil, string numberString, string[] regionCodes, out PhoneNumbers.PhoneNumber phoneNumber)
        {
            phoneNumber = null;

            foreach (var regionCode in regionCodes)
            {
                phoneNumber = ParseFromString(phoneUtil, numberString, regionCode);

                if (phoneNumber == null)
                    continue;

                if (phoneUtil.IsValidNumberForRegion(phoneNumber, regionCode))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the PhoneNumbers.PhoneNumber object if it is a valid number
        /// </summary>
        /// <param name="phoneUtil">PhoneUtil instance</param>
        /// <param name="numberString">The number to validate against</param>
        /// <param name="regionCodes">The regions to check</param>
        /// <returns>Null if phonenumber was invalid</returns>
        public static PhoneNumbers.PhoneNumber GetValidNumber(this PhoneNumbers.PhoneNumberUtil phoneUtil, string numberString, string[] regionCodes)
        {
            foreach (var regionCode in regionCodes)
            {
                var phoneNumber = ParseFromString(phoneUtil, numberString, regionCode);

                if (phoneNumber == null)
                    continue;

                if (phoneUtil.IsValidNumberForRegion(phoneNumber, regionCode))
                    return phoneNumber;
            }

            return null;
        }

        private static PhoneNumbers.PhoneNumber ParseFromString(PhoneNumbers.PhoneNumberUtil phoneUtil, string numberString, string regionCode)
        {
            try
            {
                return phoneUtil.Parse(numberString, regionCode);
            }
            catch (NumberParseException)
            {
                return null;
            }
        }

        /// <summary>
        /// Try gets the phonenumber formatted in E164 if it was valid for the first country as specified in order of countries passed in
        /// </summary>
        /// <param name="phoneUtil">PhoneNumberUtil instance</param>
        /// <param name="numberString">The phonenumber to get </param>
        /// <param name="countryCodes">The countries to check for a valid phonenumber</param>
        /// <param name="formattedPhoneNumber">The phonenumber formatted in E164</param>
        /// <returns>True if successfully retrieves the formatted phonenumber; else false</returns>
        public static bool TryGetFormattedPhoneNumber(this PhoneNumbers.PhoneNumberUtil phoneUtil, string numberString, string[] countryCodes, out string formattedPhoneNumber)
        {
            formattedPhoneNumber = null;

            foreach (var countryCode in countryCodes)
            {
                var phoneNumber = phoneUtil.Parse(numberString, countryCode);
                if (phoneUtil.IsValidNumber(phoneNumber))
                {
                    formattedPhoneNumber = phoneUtil.Format(phoneNumber, PhoneNumberFormat.E164);
                    return true;
                }
            }

            return false;
        }
    }
}