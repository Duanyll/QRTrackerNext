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

        public ObservableCollection<Homework> Homeworks { get; }
        public Command LoadHomeworksCommand { get; }
        public Command AddHomeworkCommand { get; }
        public Command<Homework> RemoveHomeworkCommand { get; }
        public Command<Homework> HomeworkTapped { get; }

        private Realm realm;

        public HomeworksViewModel()
        {
            realm = Realm.GetInstance();
            Title = "所有作业";
            Homeworks = new ObservableCollection<Homework>();
            LoadHomeworksCommand = new Command(() =>
            {
                IsBusy = true;
                try
                {
                    Homeworks.Clear();
                    var homeworksQuery = realm.All<Homework>().OrderByDescending(i => i.CreationTime);
                    foreach (var i in homeworksQuery)
                    {
                        Homeworks.Add(i);
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
                        foreach (var i in homework.Scans)
                        {
                            realm.Remove(i);
                        }
                        realm.Remove(homework);
                    });
            });
            HomeworkTapped = new Command<Homework>(OnHomeworkSelected);
        }

        private IDisposable realmToken;
        public void OnAppearing()
        {
            selectedHomework = null;
            realmToken = realm.All<Homework>().SubscribeForNotifications((sender, changes, error) =>
            {
                if (error != null)
                {
                    Debug.WriteLine(error);
                }
                LoadHomeworksCommand.Execute(null);
            });
        }

        public void OnDisappearing()
        {
            realmToken.Dispose();
        }

        async void OnHomeworkSelected(Homework homework)
        {
            if (homework == null) return;
            await Shell.Current.GoToAsync($"{nameof(HomeworkDetailPage)}?homeworkId={homework.Id}");
        }
    }
}
