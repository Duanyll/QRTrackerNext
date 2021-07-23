using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Realms;
using Acr.UserDialogs;

using Xamarin.Forms;

using QRTrackerNext.Models;
using QRTrackerNext.Views;

namespace QRTrackerNext.ViewModels
{
    class GroupsViewModel : BaseViewModel
    {
        private Group selectedGroup;
        public Group SelectedGroup
        {
            get => selectedGroup;
            set
            {
                SetProperty(ref selectedGroup, value);
                OnGroupSelected(value);
            }
        }

        public ObservableCollection<Group> Groups { get; }
        public Command LoadGroupsCommand { get; }
        public Command AddGroupCommand { get; }
        public Command<Group> UpdateGroupCommand { get; }
        public Command<Group> RemoveGroupCommand { get; }
        public Command<Group> GroupTapped { get; }

        private Realm realm;

        public GroupsViewModel()
        {
            realm = Realm.GetInstance();
            Title = "所有班级";
            Groups = new ObservableCollection<Group>();
            LoadGroupsCommand = new Command(() =>
            {
                IsBusy = true;
                try
                {
                    Groups.Clear();
                    var groupsQuery = realm.All<Group>();
                    foreach (var i in groupsQuery)
                    {
                        Groups.Add(i);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    IsBusy = false;
                }
            });
            AddGroupCommand = new Command(async () =>
            {
                var result = await UserDialogs.Instance.PromptAsync("请输入新建班级名称");
                if (result.Ok && !string.IsNullOrWhiteSpace(result.Text))
                {
                    realm.Write(() =>
                    {
                        realm.Add(new Group()
                        {
                            Name = result.Text.Trim()
                        });

                    });
                }
            });
            UpdateGroupCommand = new Command<Group>(async (group) =>
            {
                var result = await UserDialogs.Instance.PromptAsync($"将 {group.Name} 重命名为");
                if (result.Ok && !string.IsNullOrWhiteSpace(result.Text))
                {
                    realm.Write(() =>
                    {
                        group.Name = result.Text.Trim();
                    });
                }
            });
            RemoveGroupCommand = new Command<Group>(async (group) =>
            {
                var result = await UserDialogs.Instance.ConfirmAsync($"确定要删除 {group.Name} 吗");
                if (result)
                    realm.Write(() =>
                    {
                        realm.Remove(group);
                    });
            });
            GroupTapped = new Command<Group>(OnGroupSelected);


        }

        private IDisposable realmToken;
        public void OnAppearing()
        {
            IsBusy = true;
            selectedGroup = null;
            realmToken = realm.All<Group>().SubscribeForNotifications((sender, changes, error) =>
            {
                if (error != null)
                {
                    Debug.WriteLine(error);
                }
                LoadGroupsCommand.Execute(null);
            });
        }

        public void OnDisappearing()
        {
            realmToken.Dispose();
        }

        async void OnGroupSelected(Group group)
        {
            if (group == null) return;
            await Shell.Current.GoToAsync($"{nameof(StudentsPage)}?groupId={group.Id}");
        }
    }
}
