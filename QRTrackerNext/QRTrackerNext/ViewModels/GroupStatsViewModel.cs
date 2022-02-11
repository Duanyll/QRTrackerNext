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

            ExportCSVCommand = new Command(async () =>
            {
                IsBusy = true;
                if (await CheckPermission())
                {
                    var groupName = group.Name;
                    var homeworkIds = Homeworks.Where(i => i.Selected).OrderBy(i => i.Data.CreationTime).Select(i => i.Data.Id).ToArray();
                    await Task.Run(async () =>
                    {
                        var csv = QRHelper.ExportStatsCSV(ObjectId.Parse(groupId), homeworkIds);
                        var store = DependencyService.Get<IMediaStore>();
                        var path = store.SaveCSV(csv, $"{groupName}-{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.csv");
                        UserDialogs.Instance.Toast("已保存到 /sdcard/Documents/QRTracker", new TimeSpan(0, 0, 5));
                    });
                }
                IsBusy = false;
            }, () => !IsBusy && Homeworks.Any(i => i.Selected));
            PropertyChanged += (_, __) => ExportCSVCommand.ChangeCanExecute();
            Homeworks.CollectionChanged += (_, __) => ExportCSVCommand.ChangeCanExecute();
        }
    }
}
