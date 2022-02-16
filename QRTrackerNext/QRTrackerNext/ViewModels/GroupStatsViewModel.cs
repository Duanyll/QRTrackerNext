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

    class SelectableHomeworkType : SelectableData<HomeworkType>
    {
        public SelectableHomeworkType(HomeworkType type) : base(type) { }
    }

    class GroupStatsViewModel : BaseViewModel
    {
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
                hasFilterOptionChanged = true;
            }
        }

        public DateTime StatsDateBegin
        {
            get => statsDateBegin;
            set
            {
                if (value > statsDateEnd) return;
                SetProperty(ref statsDateBegin, value);
                hasFilterOptionChanged = true;
            }
        }

        public bool UseStatsDateEnd
        {
            get => useStatsDateEnd;
            set
            {
                SetProperty(ref useStatsDateEnd, value);
                hasFilterOptionChanged = true;
            }
        }

        public DateTime StatsDateEnd
        {
            get => statsDateEnd;
            set
            {
                if (value < statsDateBegin) return;
                SetProperty(ref statsDateEnd, value);
                hasFilterOptionChanged = true;
            }
        }

        Dictionary<HomeworkType, List<Homework>> typeToHomework;
        IList<SelectableHomework> filteredHomeworks = new List<SelectableHomework>();
        public IList<SelectableHomework> FilteredHomeworks
        {
            get => filteredHomeworks;
            set => SetProperty(ref filteredHomeworks, value);
        }
        public IList<SelectableHomeworkType> HomeworkTypes { get; }

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

        bool hasFilterOptionChanged = true;

        public void ResetFilterOptions()
        {
            if (hasFilterOptionChanged)
            {
                hasFilterOptionChanged = false;
                var query = HomeworkTypes.Where(i => i.Selected).SelectMany(i => typeToHomework[i.Data]);
                if (useStatsDateBegin)
                {
                    query = query.Where(i => i.CreationTime >= statsDateBegin);
                }
                if (useStatsDateEnd)
                {
                    query = query.Where(i => i.CreationTime <= statsDateEnd + TimeSpan.FromDays(1));
                }
                FilteredHomeworks = query.OrderByDescending(i => i.CreationTime)
                    .Select(i => new SelectableHomework(i) { Selected = true }).ToList();
            }
        }

        public GroupStatsViewModel(string groupId)
        {
            var realm = RealmManager.OpenDefault();
            var group = realm.Find<Group>(ObjectId.Parse(groupId));
            typeToHomework = group.Homeworks.ToList().GroupBy(i => i.Type).ToDictionary(i => i.Key, i => i.ToList());
            HomeworkTypes = typeToHomework.Keys.Select(i => new SelectableHomeworkType(i)).ToList();
            foreach (var i in HomeworkTypes)
            {
                i.PropertyChanged += (s, e) => hasFilterOptionChanged |= true;
            }

            ExportCSVCommand = new Command(async () =>
            {
                IsBusy = true;
                if (await CheckPermission())
                {
                    var groupName = group.Name;
                    ResetFilterOptions();
                    var homeworkIds = FilteredHomeworks.Where(i => i.Selected).OrderBy(i => i.Data.CreationTime).Select(i => i.Data.Id).ToArray();
                    if (homeworkIds.Length == 0)
                    {
                        await UserDialogs.Instance.AlertAsync("请检查筛选选项和选择的作业", "没有选择作业");
                    }
                    else
                    {
                        if (homeworkIds.Length > 20)
                        {
                            if (!await UserDialogs.Instance.ConfirmAsync(
                                "选择的作业数量较多，导出用时较长，并且有可能导出后CSV文件排版不佳。建议每次最多导出 20 次作业。\n还要继续导出吗？", "提示", "继续导出", "取消导出"))
                            {
                                IsBusy = false;
                                return;
                            }
                        }
                        using (UserDialogs.Instance.Loading("正在导出作业"))
                        {
                            await Task.Run(() =>
                            {
                                var csv = QRHelper.ExportStatsCSV(ObjectId.Parse(groupId), homeworkIds);
                                var store = DependencyService.Get<IMediaStore>();
                                var path = store.SaveCSV(csv, $"{groupName}-{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.csv");
                            });
                        }
                        UserDialogs.Instance.Toast("已保存到 /sdcard/Documents/QRTracker", new TimeSpan(0, 0, 5));
                    }
                }
                IsBusy = false;
            }, () => !IsBusy);
            PropertyChanged += (s, e) => ExportCSVCommand.ChangeCanExecute();
        }
    }
}
