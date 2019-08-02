using Chat.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Contacts;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Chat
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            var titlebar = ApplicationView.GetForCurrentView().TitleBar;
            var transparentColorBrush = new SolidColorBrush { Opacity = 0 };
            var transparentColor = transparentColorBrush.Color;
            titlebar.BackgroundColor = transparentColor;
            titlebar.ButtonBackgroundColor = transparentColor;
            titlebar.ButtonInactiveBackgroundColor = transparentColor;
            var solidColorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;

            if (solidColorBrush != null)
            {
                titlebar.ButtonForegroundColor = solidColorBrush.Color;
                titlebar.ButtonInactiveForegroundColor = solidColorBrush.Color;
            }

            var colorBrush = Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush;

            if (colorBrush != null)
            {
                titlebar.ForegroundColor = colorBrush.Color;
            }

            var hovercolor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 32;
            titlebar.ButtonHoverBackgroundColor = hovercolor;
            titlebar.ButtonHoverForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;
            hovercolor.A = 64;
            titlebar.ButtonPressedBackgroundColor = hovercolor;
            titlebar.ButtonPressedForegroundColor = (Application.Current.Resources["ApplicationForegroundThemeBrush"] as SolidColorBrush).Color;

            MainFrame.Navigate(typeof(ComposePage));

            Load();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var args = e.Parameter as ProtocolActivatedEventArgs;
            if (args != null)
            {
                Uri uri = args.Uri;
                string unescpateduri = Uri.UnescapeDataString(uri.Query);
                var contactid = unescpateduri.Replace("?ContactRemoteIds=", "");
                if (uri.Scheme == "ms-ipmessaging")
                {
                    ContactStore store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);
                    Contact contact = await store.GetContactAsync(contactid);

                    MainFrame.Navigate(typeof(ComposePage), contact);
                }
            }
        }

        private async void Load()
        {
            var store = await ContactManager.RequestStoreAsync();

            try
            {
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
