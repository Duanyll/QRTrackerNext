using DLToolkit.Forms.Controls;
using QRTrackerNext.Services;
using QRTrackerNext.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QRTrackerNext
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
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
