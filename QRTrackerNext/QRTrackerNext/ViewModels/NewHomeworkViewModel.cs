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

namespace QRTrackerNext.ViewModels
{
    class SelectableGroup : SelectableData<Group>
    {
        public SelectableGroup(Group group) : base(group) { }
    }

    class NewHomeworkViewModel : BaseViewModel
    {
        public Command CreateNewHomeworkCommand { get; }

        string name = $"{DateTime.Today.ToShortDateString()} 作业";
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }

        public ObservableCollection<SelectableGroup> Groups { get; }

        public IList<string> HomeworkTypeNames { get; }
        int selectedTypeIndex = -1;
        public int SelectedTypeIndex
        {
            get => selectedTypeIndex;
            set => SetProperty(ref selectedTypeIndex, value);
        }

        Realm realm;

        public NewHomeworkViewModel()
        {
            Title = "新建作业";
            realm = Services.RealmManager.OpenDefault();
            var groups = realm.All<Group>().OrderBy(i => i.NamePinyin);
            Groups = new ObservableCollection<SelectableGroup>();
            foreach (var i in groups)
            {
                Groups.Add(new SelectableGroup(i));
            }
            if (Groups.Count == 1)
            {
                Groups[0].Selected = true;
            }
            var homeworkType = realm.All<HomeworkType>().ToList();
            HomeworkTypeNames = homeworkType.Select(i => i.Name).ToList();

            CreateNewHomeworkCommand = new Command(async () =>
            {
                ObjectId createdId = ObjectId.Empty;
                realm.Write(() =>
                {
                    var homework = realm.Add(new Homework()
                    {
                        Name = Name.Trim(),
                        Type = homeworkType.ElementAt(SelectedTypeIndex)
                    });
                    foreach (var i in Groups.Where(i => i.Selected))
                    {
                        homework.Groups.Add(i.Data);
                    }
                    foreach (var i in homework.Groups.SelectMany(i => i.Students))
                    {
                        homework.Status.Add(realm.Add(new HomeworkStatus()
                        {
                            Student = i,
                            Time = homework.CreationTime,
                            Color = "gray",
                            HasScanned = false,
                            HomeworkId = homework.Id
                        }));
                    }
                    createdId = homework.Id;
                });
                await Shell.Current.GoToAsync("..");
                await Shell.Current.GoToAsync($"{nameof(HomeworkDetailPage)}?homeworkId={createdId}");
            }, () => !string.IsNullOrWhiteSpace(Name) && Groups.Any(i => i.Selected) && !Name.Contains(',') && selectedTypeIndex != -1);

            PropertyChanged += (_, __) => CreateNewHomeworkCommand.ChangeCanExecute();
            Groups.CollectionChanged += (_, __) => CreateNewHomeworkCommand.ChangeCanExecute();
        }

        public void SetHomeworkTypeById(string idString)
        {
            if (ObjectId.TryParse(idString, out var id))
            {
                var homework = realm.Find<HomeworkType>(id);
                if (homework == null) return;
                var idx = HomeworkTypeNames.IndexOf(homework.Name);
                if (idx != -1) SelectedTypeIndex = idx;
            }
        }
    }
}
