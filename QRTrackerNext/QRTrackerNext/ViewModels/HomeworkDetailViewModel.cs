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
        public int StudentCount
        {
            get => studentCount;
            set
            {
                SetProperty(ref studentCount, value);
            }
        }

        public ObservableCollection<Student> StudentsSubmitted { get; }
        public ObservableCollection<Student> StudentsNotSubmitted { get; }

        public Command LoadStudentsCommand { get; }

        public Command GoScanCommand { get; }
        public Command ManualSubmitCommand { get; }

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

            GoScanCommand = new Command(() =>
            {
                var csPage = new Views.ScanningOverlay.CustomScanPage();
                csPage.OnScanResult += (result) =>
                {
                    var res = QRHelper.ParseStudentUri(result.Text);
                    var student = realm.Find<Student>(res);
                    if (student != null && allStudents.Contains(student))
                    {
                        if (studentsNotSubmitted.Contains(student))
                        {
                            realm.Write(() =>
                            {
                                var scan = realm.Add(new ScanLog()
                                {
                                    student = student
                                });
                                homework.Scans.Add(scan);
                            });
                        }
                        csPage.ScanSuccess(student.Name);
                    }
                    else
                    {
                        csPage.ScanFailure("未知学生");
                    }
                };

                Shell.Current.Navigation.PushAsync(csPage);
            });

            ManualSubmitCommand = new Command(async () =>
            {
                var res1 = await UserDialogs.Instance.PromptAsync("输入姓氏或全名搜索需要手动录入的学生", "手动录入学生");
                if (res1.Ok && !string.IsNullOrWhiteSpace(res1.Text))
                {
                    var students = allStudents.Where(i => i.Name.StartsWith(res1.Text.Trim())).ToList();
                    if (students.Count > 0)
                    {
                        var res2 = await UserDialogs.Instance.ActionSheetAsync("选择学生", "取消", null, null, students.Select(i => i.Name).ToArray());
                        var student = students.Find(i => i.Name == res2);
                        if (student != null) {
                            if (studentsNotSubmitted.Contains(student))
                            {
                                realm.Write(() =>
                                {
                                    var scan = realm.Add(new ScanLog()
                                    {
                                        student = student
                                    });
                                    homework.Scans.Add(scan);
                                });
                            }
                            await UserDialogs.Instance.AlertAsync($"已登记 {student.Name}", "登记成功");
                        }
                    } else
                    {
                        await UserDialogs.Instance.AlertAsync($"没有找到名称包含 {res1.Text.Trim()} 的学生!", "查找失败");
                    }
                }
            });
        }

        public void OnAppearing()
        {
            LoadStudentsCommand.Execute(null);
        }
    }
}
