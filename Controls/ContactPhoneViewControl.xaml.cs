using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml.Controls;

namespace Chat.Controls
{
    public sealed partial class ContactPhoneViewControl : UserControl
    {
        public ContactPhone contactPhone
        {
            get; internal set;
        }

        public ContactPhoneViewControl(ContactPhone contactPhone, Contact contact)
        {
            InitializeComponent();
            if (contactPhone != null && contact != null)
            {
                this.contactPhone = contactPhone;
                PersonPicture.Contact = contact;
                ContactName.Text = contact.DisplayName;
                ContactPhone.Text = contactPhone.Number + " (" + contactPhone.Kind.ToString() + ")";
            }
        }

        public override string ToString()
        {
            return contactPhone.Number;
        }
    }
}
