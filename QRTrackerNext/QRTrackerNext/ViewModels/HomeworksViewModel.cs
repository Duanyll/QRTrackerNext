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
    //NOTE: 为了保持统一，Homework 用了复数。我知道他不可数！！！
    class HomeworksViewModel : BaseViewModel
    {
        private Homework selectedHomework;
        public Homework SelectedHomework
        {
            get => selectedHomework;
            set
            {
                SetProperty(ref selectedHomework, value);
                OnHomeworkSelected(value);
            }
        }

        public IQueryable<Homework> Homeworks { get; }
        public Command AddHomeworkCommand { get; }
        public Command<Homework> RemoveHomeworkCommand { get; }
        public Command<Homework> HomeworkTapped { get; }

        private Realm realm;

        public HomeworksViewModel()
        {
            realm = Services.RealmManager.OpenDefault();
            Title = "所有作业";
            Homeworks = realm.All<Homework>().OrderByDescending(i => i.CreationTime);
            AddHomeworkCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(NewHomeworkPage)}");
            });
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
            HomeworkTapped = new Command<Homework>(OnHomeworkSelected);
        }

        async void OnHomeworkSelected(Homework homework)
        {
            if (homework == null) return;
            await Shell.Current.GoToAsync($"{nameof(HomeworkDetailPage)}?homeworkId={homework.Id}");
        }
    }
}
