using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

using QRTrackerNext.Views;

namespace QRTrackerNext.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "关于";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://qrt.duanyll.com"));
            SettingsCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(SettingsPage)));
        }

        public Command OpenWebCommand { get; }
        public Command SettingsCommand { get; }
    }
}