using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Realms;
using MongoDB.Bson;
using Acr.UserDialogs;

using Xamarin.Forms;

using QRTrackerNext.Models;
using QRTrackerNext.Views;
using System.Collections.Generic;

namespace QRTrackerNext.ViewModels
{
    class HomeworkHomeViewModel : BaseViewModel
    {
        public ObservableCollection<Homework> TopHomeworks { get;}
        public Command OpenAllHomeworkListCommand { get; }
        public Command CreateHomeworkCommand { get; }
        public Command OpenHomeworkTypeListCommand { get; }
        public Command UpdateTopHomeworkCommand { get; }
        public Command<Homework> OpenHomeworkCommand { get; }

        public HomeworkHomeViewModel()
        {
            Title = "作业";
            var realm = Services.RealmManager.OpenDefault();
            TopHomeworks = new ObservableCollection<Homework>();
            OpenAllHomeworkListCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(HomeworksPage));
            });
            CreateHomeworkCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(NewHomeworkPage));
            });
            OpenHomeworkTypeListCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync(nameof(HomeworkTypesPage));
            });
            UpdateTopHomeworkCommand = new Command(() =>
            {
                TopHomeworks.Clear();
                foreach (var homework in realm.All<Homework>().OrderByDescending(i => i.CreationTime))
                {
                    TopHomeworks.Add(homework);
                    if (TopHomeworks.Count > 5)
                    {
                        break;
                    }
                }
            });
            OpenHomeworkCommand = new Command<Homework>(async (homework) =>
            {
                await Shell.Current.GoToAsync($"{nameof(HomeworkDetailPage)}?homeworkId={homework.Id}");
            });
        }

        public void OnAppearing()
        {
            UpdateTopHomeworkCommand.Execute(null);
        }
    }
}
