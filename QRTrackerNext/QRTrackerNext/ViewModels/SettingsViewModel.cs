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
using QRTrackerNext.Services;
using QRTrackerNext.Views;


namespace QRTrackerNext.ViewModels
{
    class SettingsViewModel : BaseViewModel
    {
        public Settings Settings { get; } = Settings.Instance;
    }
}
