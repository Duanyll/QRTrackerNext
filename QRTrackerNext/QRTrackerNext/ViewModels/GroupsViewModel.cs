using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Realms;
using Acr.UserDialogs;

using Xamarin.Forms;
using TinyPinyin.Core;

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

        public IQueryable<Group> Groups { get; }
        public Command AddGroupCommand { get; }
        public Command<Group> UpdateGroupCommand { get; }
        public Command<Group> RemoveGroupCommand { get; }
        public Command<Group> OpenGroupCommand { get; }

        private Realm realm;

        public GroupsViewModel()
        {
            realm = Services.RealmManager.OpenDefault();
            Title = "所有班级";
            Groups = realm.All<Group>().OrderBy(i => i.NamePinyin);
            AddGroupCommand = new Command(async () =>
            {
                var result = await UserDialogs.Instance.PromptAsync("请输入新建班级名称", "新建班级");
                if (result.Ok)
                {
                    if (string.IsNullOrWhiteSpace(result.Text) || result.Text.Contains(','))
                    {
                        await UserDialogs.Instance.AlertAsync("请输入有效的名称, 不能包含逗号", "错误");
                        return;
                    }
                    var sameName = realm.All<Group>().Where(i => i.Name == result.Text.Trim()).Count();
                    if (sameName != 0)
                    {
                        UserDialogs.Instance.Alert("已经有相同名称的班级了", "创建失败", "确认");
                        return;
                    }
                    realm.Write(() =>
                    {
                        realm.Add(new Group()
                        {
                            Name = result.Text.Trim(),
                        });

                    });
                }
            });
            UpdateGroupCommand = new Command<Group>(async (group) =>
            {
                var result = await UserDialogs.Instance.PromptAsync($"将 {group.Name} 重命名为", "重命名班级");
                if (result.Ok)
                {
                    if (string.IsNullOrWhiteSpace(result.Text) || result.Text.Contains(','))
                    {
                        await UserDialogs.Instance.AlertAsync("请输入有效的名称, 不能包含逗号", "错误");
                        return;
                    }
                    var sameName = realm.All<Group>().Where(i => i.Name == result.Text.Trim() && i.Id != group.Id).Count();
                    if (sameName != 0) 
                    {
                        UserDialogs.Instance.Alert("已经有相同名称的班级了", "保存失败", "确认");
                        return;
                    }
                    realm.Write(() =>
                    {
                        group.Name = result.Text.Trim();
                    });
                }
            });
            RemoveGroupCommand = new Command<Group>(async (group) =>
            {
                var result = await UserDialogs.Instance.ConfirmAsync($"确定要删除 {group.Name} 吗", "删除班级");
                if (result)
                {
                    if (group.Homeworks.Count() > 0) 
                    {
                        await UserDialogs.Instance.AlertAsync($"不能删除 {group.Name}, 因为还有布置给该班级的作业。请先删除这些作业。", "删除失败");
                        return;
                    }
                    realm.Write(() =>
                    {
                        foreach (var i in group.Students)
                        {
                            realm.Remove(i);
                        }
                        realm.Remove(group);
                    });
                }
            });
            OpenGroupCommand = new Command<Group>(OnGroupSelected);
        }

        async void OnGroupSelected(Group group)
        {
            if (group == null) return;
            await Shell.Current.GoToAsync($"{nameof(StudentsPage)}?groupId={group.Id}");
        }
    }
}
