using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Realms;
using Acr.UserDialogs;

using Xamarin.Forms;
using Xamarin.Essentials;

using QRTrackerNext.Models;
using QRTrackerNext.Views;


namespace QRTrackerNext.ViewModels
{
    class SettingsViewModel : BaseViewModel
    {
        private string nameRed = Preferences.Get("name_red", "");
        private string nameYellow = Preferences.Get("name_yellow", "");
        private string nameGreen = Preferences.Get("name_green", "");
        private string nameBlue = Preferences.Get("name_blue", "");
        private string namePurple = Preferences.Get("name_purple", "");

        public string NameRed
        {
            get => nameRed;
            set {
                SetProperty(ref nameRed, value);
                Preferences.Set("name_red", value);
            }
        }

        public string NameYellow
        {
            get => nameYellow;
            set
            {
                SetProperty(ref nameYellow, value);
                Preferences.Set("name_yellow", value);
            }
        }

        public string NameGreen
        {
            get => nameGreen;
            set
            {
                SetProperty(ref nameGreen, value);
                Preferences.Set("name_green", value);
            }
        }

        public string NameBlue
        {
            get => nameBlue;
            set
            {
                SetProperty(ref nameBlue, value);
                Preferences.Set("name_blue", value);
            }
        }

        public string NamePurple
        {
            get => namePurple;
            set
            {
                SetProperty(ref namePurple, value);
                Preferences.Set("name_purple", value);
            }
        }
    }
}
