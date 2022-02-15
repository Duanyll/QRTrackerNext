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
    class HomeworksByTypeViewModel : BaseViewModel
    {
        public HomeworkType HomeworkType { get; }
        public IQueryable<Homework> Homeworks { get; }
        public Command<Homework> OpenHomeworkCommand { get; }
        public Command<Homework> RemoveHomeworkCommand { get; }
        public Command AddHomeworkCommand { get; }
        public Command EditHomeworkTypeCommand { get; }

        public HomeworksByTypeViewModel(string idString)
        {
            var realm = Services.RealmManager.OpenDefault();
            var id = ObjectId.Parse(idString);
            HomeworkType = realm.Find<HomeworkType>(id);
            Homeworks = realm.All<Homework>().Where(i => i.Type == HomeworkType).OrderByDescending(i => i.CreationTime);

            OpenHomeworkCommand = new Command<Homework>(async (homework) => await Shell.Current.GoToAsync($"{nameof(HomeworkDetailPage)}?homeworkId={homework.Id}"));
            RemoveHomeworkCommand = new Command<Homework>(async (homework) =>
            {
                var result = await UserDialogs.Instance.ConfirmAsync($"确定要删除 {homework.Name} 吗", "删除作业");
                if (result)
                    realm.Write(() =>
                    {
                        foreach (var i in homework.Status)
                        {
                            realm.Remove(i);
                        }
                        realm.Remove(homework);
                    });
            });
            AddHomeworkCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(NewHomeworkPage)}?typeId={HomeworkType.Id}");
            });
            EditHomeworkTypeCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(EditHomeworkTypePage)}?typeId={HomeworkType.Id}");
            });
        }

    }
}
