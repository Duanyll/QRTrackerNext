using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace QRTrackerNext.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            LoginCommand = new Command(async () => await Shell.Current.GoToAsync("//LoginPage"));
        }

        public ICommand LoginCommand { get; }
    }
}