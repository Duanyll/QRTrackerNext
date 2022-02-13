using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Realms;
using MongoDB.Bson;
using Acr.UserDialogs;

using Xamarin.Forms;
using Xamarin.Essentials;

using QRTrackerNext.Models;
using QRTrackerNext.Views;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using QRTrackerNext.Services;

namespace QRTrackerNext.ViewModels
{
    class SelectableHomework : SelectableData<Homework>
    {
        public SelectableHomework(Homework homework) : base(homework) { }
    }

    class GroupStatsViewModel : BaseViewModel
    {
        public ObservableCollection<SelectableHomework> Homeworks { get; }

        public Command SelectAllCommand { get; }
        public Command SelectNoneCommand { get; }
        public Command SelectDefaultTimeRange { get; }

        public Command ExportCSVCommand { get; }

        private async Task<bool> CheckPermission()
        {
            var current = CrossPermissions.Current;
            var status = await current.CheckPermissionStatusAsync<StoragePermission>();
            if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                status = await current.RequestPermissionAsync<StoragePermission>();
            }
            return status == Plugin.Permissions.Abstractions.PermissionStatus.Granted;
        }

        public GroupStatsViewModel(string groupId)
        {
            var realm = RealmManager.OpenDefault();
            var group = realm.Find<Group>(ObjectId.Parse(groupId));
            Homeworks = new ObservableCollection<SelectableHomework>();
            foreach (var i in group.Homeworks.OrderByDescending(i => i.CreationTime))
            {
                Homeworks.Add(new SelectableHomework(i) { Selected = true }); 
            }

            SelectAllCommand = new Command(() =>
            {
                foreach (var i in Homeworks)
                {
                    i.Selected = true;
                }
            });

            SelectNoneCommand = new Command(() =>
            {
                foreach (var i in Homeworks)
                {
                    i.Selected = false;
                }
            });

            SelectDefaultTimeRange = new Command(async () =>
            {
                var rangeConfigured = false;
                var rangeStart = DateTimeOffset.MinValue;
                var rangeEnd = DateTimeOffset.Now + TimeSpan.FromDays(1);
                if (Preferences.Get("use_stats_date_begin", false))
                {
                    rangeStart = Preferences.Get("stats_date_begin", rangeStart.LocalDateTime);
                    rangeConfigured = true;
                }
                if (Preferences.Get("use_stats_date_end", false))
                {
                    rangeEnd = Preferences.Get("stats_date_end", rangeEnd.LocalDateTime);
                    rangeConfigured = true;
                }
                if (!rangeConfigured)
                {
                    await UserDialogs.Instance.AlertAsync("您可以在设置页面设置学期时间", "未设置学期", "确定");
                }
                else
                {
                    var hasSelected = false;
                    foreach (var i in Homeworks)
                    {
                        i.Selected = (i.Data.CreationTime >= rangeStart && i.Data.CreationTime <= rangeEnd);
                        if (i.Selected) hasSelected = true;
                    }
                    if (!hasSelected)
                    {
                        await UserDialogs.Instance.AlertAsync("请确认学期时间设置正确", "学期内没有作业", "确定");
                    }
                }
            });

            ExportCSVCommand = new Command(async () =>
            {
                IsBusy = true;
                if (await CheckPermission())
                {
                    var groupName = group.Name;
                    var homeworkIds = Homeworks.Where(i => i.Selected).OrderBy(i => i.Data.CreationTime).Select(i => i.Data.Id).ToArray();
                    await Task.Run(() =>
                    {
                        var csv = QRHelper.ExportStatsCSV(ObjectId.Parse(groupId), homeworkIds);
                        var store = DependencyService.Get<IMediaStore>();
                        var path = store.SaveCSV(csv, $"{groupName}-{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.csv");
                    });
                    UserDialogs.Instance.Toast("已保存到 /sdcard/Documents/QRTracker", new TimeSpan(0, 0, 5));
                }
                IsBusy = false;
            }, () => !IsBusy && Homeworks.Any(i => i.Selected));
            PropertyChanged += (_, __) => ExportCSVCommand.ChangeCanExecute();
            Homeworks.CollectionChanged += (_, __) => ExportCSVCommand.ChangeCanExecute();
        }
    }
}
