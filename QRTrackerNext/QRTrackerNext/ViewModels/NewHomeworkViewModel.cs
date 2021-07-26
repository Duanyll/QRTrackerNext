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

        public NewHomeworkViewModel()
        {
            Title = "新建作业";
            var realm = Realm.GetInstance();
            var groups = realm.All<Group>().OrderBy(i => i.Name);
            Groups = new ObservableCollection<SelectableGroup>();
            foreach (var i in groups)
            {
                Groups.Add(new SelectableGroup(i));
            }

            CreateNewHomeworkCommand = new Command(async () =>
            {
                realm.Write(() =>
                {
                    var homework = realm.Add(new Homework() { Name = Name.Trim() });
                    foreach (var i in Groups.Where(i => i.Selected))
                    {
                        homework.Groups.Add(i.Data);
                    }
                });
                await Shell.Current.GoToAsync("..");
            }, () => !string.IsNullOrWhiteSpace(Name) && Groups.Any(i => i.Selected));

            PropertyChanged += (_, __) => CreateNewHomeworkCommand.ChangeCanExecute();
            Groups.CollectionChanged += (_, __) => CreateNewHomeworkCommand.ChangeCanExecute();
        }
    }
}
