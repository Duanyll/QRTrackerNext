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

namespace QRTrackerNext.ViewModels
{
    class HomeworkDetailViewModel : BaseViewModel
    {
        Realm realm;
        Homework homework;
        public Homework Homework
        {
            get => homework;
            set
            {
                SetProperty(ref homework, value);
            }
        }

        int studentCount = 0;
        public int StudentCount {
            get => studentCount;
            set
            {
                SetProperty(ref studentCount, value);
            }
        }

        public ObservableCollection<Student> StudentsSubmitted { get; }
        public ObservableCollection<Student> StudentsNotSubmitted { get; }

        public Command LoadStudentsCommand { get; }

        public HomeworkDetailViewModel(string homeworkId)
        {
            realm = Realm.GetInstance();
            Homework = realm.Find<Homework>(ObjectId.Parse(homeworkId));
            StudentsSubmitted = new ObservableCollection<Student>();
            StudentsNotSubmitted = new ObservableCollection<Student>();
            if (homework == null)
            {
                Title = "作业未找到";
                return;
            }
            Title = homework.Name;

            var allStudents = homework.Groups.SelectMany(i => i.Students);
            var submittedStudents = homework.Scans.Select(i => i.student);
            var studentsNotSubmitted = allStudents.Where(i => !submittedStudents.Contains(i));

            LoadStudentsCommand = new Command(() =>
            {
                StudentCount = allStudents.Count();

                StudentsSubmitted.Clear();
                foreach (var i in submittedStudents)
                {
                    StudentsSubmitted.Add(i);
                }

                StudentsNotSubmitted.Clear();
                foreach (var i in studentsNotSubmitted)
                {
                    StudentsNotSubmitted.Add(i);
                }

                IsBusy = false;
            });
        }

        public void OnAppearing()
        {
            LoadStudentsCommand.Execute(null);
        }
    }
}
