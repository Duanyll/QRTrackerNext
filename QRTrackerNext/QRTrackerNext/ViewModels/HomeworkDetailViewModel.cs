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
        public ObservableCollection<ScanLog> ScanLogs { get; }
        public ObservableCollection<Student> StudentsNotSubmitted { get; }

        public Command LoadStudentsCommand { get; }

        public Command GoScanCommand { get; }
        public Command SearchStudentCommand { get; }
        public Command<Student> SubmitStudentCommand { get; }
        public Command<ScanLog> EditScanLogCommand { get; }

        public HomeworkDetailViewModel(string homeworkId)
        {
            realm = Services.RealmManager.OpenDefault();
            Homework = realm.Find<Homework>(ObjectId.Parse(homeworkId));
            StudentsSubmitted = new ObservableCollection<Student>();
            StudentsNotSubmitted = new ObservableCollection<Student>();
            ScanLogs = new ObservableCollection<ScanLog>();
            if (homework == null)
            {
                Title = "作业未找到";
                return;
            }
            Title = homework.Name;

            var allStudents = homework.Groups.SelectMany(i => i.Students);
            var submittedStudents = homework.Scans.Select(i => i.Student);
            var studentsNotSubmitted = allStudents.Where(i => !submittedStudents.Contains(i));
            var colors = homework.Colors.ToList();
            var convertor = new ColorChineseNameConvertor();

            ScanLog addScanLog(Student student)
            {
                if (studentsNotSubmitted.Contains(student))
                {
                    ScanLog res = null;
                    realm.Write(() =>
                    {
                        var scan = realm.Add(new ScanLog()
                        {
                            Student = student
                        });
                        homework.Scans.Add(scan);
                        res = scan;
                    });
                    return res;
                }
                else
                {
                    return homework.Scans.First(i => i.Student.Id == student.Id);
                }
            }

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

                ScanLogs.Clear();
                foreach (var i in homework.Scans)
                {
                    ScanLogs.Add(i);
                }

                IsBusy = false;
            });

            GoScanCommand = new Command(() =>
            {
                var csPage = new Views.ScanningOverlay.CustomScanPage(colors);
                csPage.OnScanResult += (result) =>
                {
                    var res = QRHelper.ParseStudentUri(result.Text);
                    var student = realm.Find<Student>(res);
                    if (student != null && allStudents.Contains(student))
                    {
                        var scanLog = addScanLog(student);
                        csPage.ScanSuccess(student.Name);
                        if (colors.Count > 0)
                        {
                            csPage.RequestRateLastScan((color) =>
                            {
                                realm.Write(() =>
                                {
                                    scanLog.Color = color;
                                });
                                UserDialogs.Instance.Toast($"已将 {student.Name} 登记为 {convertor.Convert(color, null, null, null)}", new TimeSpan(0, 0, 3));
                            });
                        }
                    }
                    else
                    {
                        csPage.ScanFailure("未知学生");
                    }
                };

                Shell.Current.Navigation.PushAsync(csPage);
            });

            async void dialogSubmitStudent(Student student)
            {
                var scanLog = addScanLog(student);
                if (colors.Count > 0)
                {
                    var color = convertor.ConvertBack(
                            await UserDialogs.Instance.ActionSheetAsync("选择登记颜色", "不标记颜色", null, null,
                                colors.Select(i => convertor.Convert(i, null, null, null) as string).ToArray()),
                            null, null, null) as string;
                    realm.Write(() =>
                    {
                        scanLog.Color = color;
                    });
                    UserDialogs.Instance.Toast($"已登记 {student.Name}, 颜色为 { convertor.Convert(color, null, null, null) }", new TimeSpan(0, 0, 3));
                }
                else
                {
                    UserDialogs.Instance.Toast($"已登记 {student.Name}", new TimeSpan(0, 0, 3));
                }
                LoadStudentsCommand.Execute(null);
            }

            SearchStudentCommand = new Command(async () =>
            {
                var res1 = await UserDialogs.Instance.PromptAsync("输入姓氏或全名", "查找学生");
                if (res1.Ok && !string.IsNullOrWhiteSpace(res1.Text))
                {
                    var students = allStudents.Where(i => i.Name.StartsWith(res1.Text.Trim())).ToList();
                    if (students.Count > 0)
                    {
                        var res2 = await UserDialogs.Instance.ActionSheetAsync("选择学生", "取消", null, null, students.Select(i => i.Name).ToArray());
                        var student = students.Find(i => i.Name == res2);
                        if (student != null)
                        {
                            if (studentsNotSubmitted.Contains(student))
                            {
                                SubmitStudentCommand.Execute(student);
                            } 
                            else
                            {
                                EditScanLogCommand.Execute(homework.Scans.First(i => i.Student.Id == student.Id));
                            }
                        }
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync($"没有找到名称包含 {res1.Text.Trim()} 的学生!", "查找失败");
                    }
                }
            });

            SubmitStudentCommand = new Command<Student>(async (student) =>
            {
                if (await UserDialogs.Instance.ConfirmAsync($"要登记 {student.Name} 吗?", $"{student.Name} 还没有登记"))
                {
                    dialogSubmitStudent(student);
                }
            });

            EditScanLogCommand = new Command<ScanLog>(async (scanLog) =>
            {
                if (colors.Count > 0)
                {
                    var res = await UserDialogs.Instance.ActionSheetAsync(
                        $"{scanLog.Student.Name} 已登记为 {convertor.Convert(scanLog.Color, null, null, null)}", "好", "删除登记", null,
                        colors.Select(i => convertor.Convert(i, null, null, null) as string).ToArray());
                    if (res == "删除登记")
                    {
                        UserDialogs.Instance.Toast($"已删除 {scanLog.Student.Name} 登记的作业", new TimeSpan(0, 0, 3));
                        realm.Write(() =>
                        {
                            realm.Remove(scanLog);
                        });
                    }
                    else if (!string.IsNullOrEmpty(res) && res != "好")
                    {
                        var color = convertor.ConvertBack(res, null, null, null) as string;
                        realm.Write(() =>
                        {
                            scanLog.Color = color;
                        });
                        UserDialogs.Instance.Toast($"已将 {scanLog.Student.Name} 的颜色改为 { convertor.Convert(color, null, null, null) }", new TimeSpan(0, 0, 3));
                    }
                    LoadStudentsCommand.Execute(null);
                }
                else
                {
                    if (await UserDialogs.Instance.ConfirmAsync($"要删除 {scanLog.Student.Name} 登记的作业吗", $"{scanLog.Student.Name} 已经登记了"))
                    {
                        UserDialogs.Instance.Toast($"已删除 {scanLog.Student.Name} 登记的作业", new TimeSpan(0, 0, 3));
                        realm.Write(() =>
                        {
                            realm.Remove(scanLog);
                        });
                        LoadStudentsCommand.Execute(null);
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
