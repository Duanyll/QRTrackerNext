using DLToolkit.Forms.Controls;
using QRTrackerNext.Services;
using QRTrackerNext.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace QRTrackerNext
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            VersionTracking.Track();
            FlowListView.Init();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
