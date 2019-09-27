﻿using Chat.Helpers;
using Windows.ApplicationModel;

namespace Chat.ViewModels
{
    public class AboutViewModel : Observable
    {
        // Properties
        private string _appName;
        public string AppName
        {
            get { return _appName; }
            set { Set(ref _appName, value); }
        }

        private string _versionNumber;
        public string VersionNumber
        {
            get { return _versionNumber; }
            set { Set(ref _versionNumber, value); }
        }


        // Constructor
        public AboutViewModel()
        {
            Initialize();
        }

        // Initialize Stuff
        public void Initialize()
        {
            AppName = GetAppName();
            VersionNumber = GetVersionNumber();
        }


        // Methods
        private static string GetAppName()
        {
            var package = Package.Current;
            string appName = package.DisplayName;

            return appName;
        }

        private static string GetVersionNumber()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
