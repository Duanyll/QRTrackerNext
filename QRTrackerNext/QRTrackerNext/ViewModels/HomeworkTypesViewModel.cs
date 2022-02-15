using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Realms;
using Acr.UserDialogs;

using Xamarin.Forms;
using MongoDB.Bson;

using QRTrackerNext.Models;
using QRTrackerNext.Views;

namespace QRTrackerNext.ViewModels
{
    internal class HomeworkTypesViewModel : BaseViewModel
    {
        public IRealmCollection<HomeworkType> HomeworkTypes { get; }
        public Command AddHomeworkTypeCommand { get; }
        public Command<HomeworkType> EditHomeworkTypeCommand { get; }
        public Command<HomeworkType> RemoveHomeworkTypeCommand { get; }
        public Command<HomeworkType> OpenHomeworkListCommand { get; }

        public HomeworkTypesViewModel()
        {
            Title = "作业分类";
            var realm = Services.RealmManager.OpenDefault();
            HomeworkTypes = realm.All<HomeworkType>().AsRealmCollection();
            AddHomeworkTypeCommand = new Command(async () =>
            {
                var res = await UserDialogs.Instance.PromptAsync("输入新建作业分类名称", "创建作业分类");
                if (res.Ok && !string.IsNullOrEmpty(res.Text))
                {
                    var sameName = HomeworkTypes.Where(i => i.Name == res.Text.Trim()).Count();
                    if (sameName > 0)
                    {
                        await UserDialogs.Instance.AlertAsync("已经有这个名称的分类了", "创建失败");
                        return;
                    }
                    realm.Write(() =>
                    {
                        realm.Add(new HomeworkType()
                        {
                            Name = res.Text.Trim(),
                        });
                    });
                }
            });
            EditHomeworkTypeCommand = new Command<HomeworkType>(async (homeworkType) =>
            {
                await Shell.Current.GoToAsync($"{nameof(EditHomeworkTypePage)}?typeId={homeworkType.Id}");
            });
            RemoveHomeworkTypeCommand = new Command<HomeworkType>(async (homeworkType) =>
            {
                if (homeworkType.Homeworks.Count() > 0)
                {
                    await UserDialogs.Instance.AlertAsync("还有作业属于这个分类, 只能删除没有作业的分类", "删除失败");
                    return;
                }
                if (await UserDialogs.Instance.ConfirmAsync($"确定要删除 {homeworkType.Name} 吗", "删除分类"))
                {
                    realm.Write(() =>
                    {
                        realm.Remove(homeworkType);
                    });
                }
            });
            OpenHomeworkListCommand = new Command<HomeworkType>(async (homeworkType) =>
            {
                await Shell.Current.GoToAsync($"{nameof(HomeworksByTypePage)}?typeId={homeworkType.Id}");
            });
        }
    }
}
