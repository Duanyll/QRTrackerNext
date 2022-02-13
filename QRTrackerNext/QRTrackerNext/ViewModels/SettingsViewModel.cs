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
                if (value?.Contains(',') ?? false) return; 
                SetProperty(ref nameRed, value);
                Preferences.Set("name_red", value);
            }
        }

        public string NameYellow
        {
            get => nameYellow;
            set
            {
                if (value?.Contains(',') ?? false) return;
                SetProperty(ref nameYellow, value);
                Preferences.Set("name_yellow", value);
            }
        }

        public string NameGreen
        {
            get => nameGreen;
            set
            {
                if (value?.Contains(',') ?? false) return;
                SetProperty(ref nameGreen, value);
                Preferences.Set("name_green", value);
            }
        }

        public string NameBlue
        {
            get => nameBlue;
            set
            {
                if (value?.Contains(',') ?? false) return;
                SetProperty(ref nameBlue, value);
                Preferences.Set("name_blue", value);
            }
        }

        public string NamePurple
        {
            get => namePurple;
            set
            {
                if (value?.Contains(',') ?? false) return;
                SetProperty(ref namePurple, value);
                Preferences.Set("name_purple", value);
            }
        }

        private bool useStatsDateBegin = Preferences.Get("use_stats_date_begin", false);
        private DateTime statsDateBegin = Preferences.Get("stats_date_begin", DateTime.Today - TimeSpan.FromDays(90));
        private bool useStatsDateEnd = Preferences.Get("use_stats_date_end", false);

        private DateTime statsDateEnd = Preferences.Get("stats_date_end", DateTime.Today);
        public bool UseStatsDateBegin
        {
            get => useStatsDateBegin;
            set
            {
                SetProperty(ref useStatsDateBegin, value);
                Preferences.Set("use_stats_date_begin", value);
                if (!Preferences.ContainsKey("stats_date_begin"))
                {
                    statsDateBegin = DateTime.Today - TimeSpan.FromDays(90);
                }
                if (!Preferences.ContainsKey("stats_date_end"))
                {
                    statsDateEnd = DateTime.Today;
                }
            }
        }

        public DateTime StatsDateBegin
        {
            get => statsDateBegin;
            set {
                if (value > statsDateEnd) return;
                SetProperty(ref statsDateBegin, value);
                Preferences.Set("stats_date_begin", value);
            }
        }

        public bool UseStatsDateEnd
        {
            get => useStatsDateEnd;
            set
            {
                SetProperty(ref useStatsDateEnd, value);
                Preferences.Set("use_stats_date_end", value);
                if (!Preferences.ContainsKey("stats_date_begin"))
                {
                    statsDateBegin = DateTime.Today - TimeSpan.FromDays(90);
                }
                if (!Preferences.ContainsKey("stats_date_end"))
                {
                    statsDateEnd = DateTime.Today;
                }
            }
        }

        public DateTime StatsDateEnd
        {
            get => statsDateEnd;
            set
            {
                if (value < statsDateBegin) return;
                SetProperty(ref statsDateEnd, value);
                Preferences.Set("stats_date_end", value);
            }
        }
    }
}
