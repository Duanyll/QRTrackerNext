﻿using System;
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

    class SelectableColorName : SelectableData<string>
    {
        public SelectableColorName(string str) : base(str) { }
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
        public ObservableCollection<SelectableColorName> ColorNames { get; }

        public NewHomeworkViewModel()
        {
            Title = "新建作业";
            var realm = Services.RealmManager.OpenDefault();
            var groups = realm.All<Group>().OrderBy(i => i.Name);
            Groups = new ObservableCollection<SelectableGroup>();
            foreach (var i in groups)
            {
                Groups.Add(new SelectableGroup(i));
            }

            ColorNames = new ObservableCollection<SelectableColorName>()
            {
                new SelectableColorName("red") { Selected = true },
                new SelectableColorName("yellow") { Selected = true },
                new SelectableColorName("green") { Selected = true },
                new SelectableColorName("blue"),
                new SelectableColorName("purple")
            };

            CreateNewHomeworkCommand = new Command(async () =>
            {
                realm.Write(() =>
                {
                    var homework = realm.Add(new Homework() { Name = Name.Trim() });
                    foreach (var i in Groups.Where(i => i.Selected))
                    {
                        homework.Groups.Add(i.Data);
                    }
                    foreach (var i in ColorNames.Where(i => i.Selected)) 
                    {
                        homework.Colors.Add(i.Data);
                    }
                    foreach (var i in homework.Groups.SelectMany(i => i.Students))
                    {
                        homework.Status.Add(realm.Add(new HomeworkStatus()
                        {
                            Student = i,
                            Time = homework.CreationTime,
                            Color = "grey",
                            HasScanned = false,
                            HomeworkId = homework.Id
                        }));
                    }
                });
                await Shell.Current.GoToAsync("..");
            }, () => !string.IsNullOrWhiteSpace(Name) && Groups.Any(i => i.Selected) && !Name.Contains(','));

            PropertyChanged += (_, __) => CreateNewHomeworkCommand.ChangeCanExecute();
            Groups.CollectionChanged += (_, __) => CreateNewHomeworkCommand.ChangeCanExecute();
        }
    }
}
