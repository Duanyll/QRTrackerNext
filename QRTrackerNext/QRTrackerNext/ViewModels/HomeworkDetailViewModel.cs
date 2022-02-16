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
using Microcharts;

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

        string name;
        public string Name
        {
            get => name;
            set
            {
                if (!string.IsNullOrEmpty(value) && homework != null)
                {
                    SetProperty(ref name, value);
                    realm.Write(() => homework.Name = name);
                    Title = value;
                }
            }
        }

        public IQueryable<HomeworkStatus> SubmittedStatus { get; set; }
        public IQueryable<HomeworkStatus> NotSubmittedStatus { get; set; }

        public Command GoScanCommand { get; }
        public Command SearchStudentCommand { get; }
        public Command<Student> SubmitStudentCommand { get; }
        public Command<HomeworkStatus> EditScanLogCommand { get; }
        public Command EditHomeworkTypeCommand { get; }

        public Command ChangeHomeworkTypeCommand { get; }

        public HomeworkDetailViewModel(string homeworkId)
        {
            realm = Services.RealmManager.OpenDefault();
            Homework = realm.Find<Homework>(ObjectId.Parse(homeworkId));
            if (homework == null)
            {
                Title = "作业未找到";
                return;
            }
            Title = homework.Name;
            name = homework.Name;

            var statusIndex = new Dictionary<ObjectId, HomeworkStatus>();
            var allStudents = new List<Student>();
            foreach (var status in homework.Status)
            {
                if (status.Student == null) continue;
                statusIndex.Add(status.Student.Id, status);
                allStudents.Add(status.Student);
            }
            StudentCount = allStudents.Count;
            SubmittedStatus = homework.Status.Where(i => i.HasScanned == true).OrderBy(i => i.Student.NamePinyin).AsQueryable();
            NotSubmittedStatus = homework.Status.Where(i => i.HasScanned == false).OrderBy(i => i.Student.NamePinyin).AsQueryable();
            var colors = homework.Type.Colors.ToList();

            GoScanCommand = new Command(() =>
            {
                var csPage = new Views.ScanningOverlay.CustomScanPage(colors);
                csPage.OnScanResult += (result) =>
                {
                    var res = QRHelper.ParseStudentUri(result.Text);
                    if (statusIndex.TryGetValue(res, out HomeworkStatus status))
                    {
                        csPage.ScanSuccess(status.Student.Name);
                        realm.Write(() =>
                        {
                            status.HasScanned = true;
                            status.Time = DateTimeOffset.Now;
                        });
                        if (colors.Count > 0)
                        {
                            csPage.RequestRateLastScan((color) =>
                            {
                                realm.Write(() =>
                                {
                                    status.Color = color;
                                });
                                UserDialogs.Instance.Toast($"已将 {status.Student.Name} 登记为 {LabelUtils.NameToChineseDisplay(color, homework.Type)}", new TimeSpan(0, 0, 3));
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
                            if (statusIndex[student.Id].HasScanned)
                            {
                                EditScanLogCommand.Execute(homework.Status.First(i => i.Student.Id == student.Id));
                            }
                            else
                            {
                                SubmitStudentCommand.Execute(student);
                            }
                        }
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync($"没有找到名称包含 {res1.Text.Trim()} 的学生.", "查找失败");
                    }
                }
            });

            SubmitStudentCommand = new Command<Student>(async (student) =>
            {
                if (statusIndex.TryGetValue(student.Id, out HomeworkStatus status))
                {
                    if (await UserDialogs.Instance.ConfirmAsync($"要登记 {student.Name} 吗?", $"{student.Name} 还没有登记"))
                    {
                        realm.Write(() =>
                        {
                            status.HasScanned = true;
                            status.Time = DateTimeOffset.Now;
                        });
                        if (colors.Count > 0)
                        {
                            var color = LabelUtils.ChineseDisplayToName(
                                    await UserDialogs.Instance.ActionSheetAsync("选择登记颜色", "不标记颜色", null, null,
                                        colors.Select(i => LabelUtils.NameToChineseDisplay(i, homework.Type)).ToArray()));
                            realm.Write(() =>
                            {
                                status.Color = color;
                            });
                            UserDialogs.Instance.Toast($"已登记 {student.Name}, 颜色为 {LabelUtils.NameToChineseDisplay(color, homework.Type)}", new TimeSpan(0, 0, 3));
                        }
                        else
                        {
                            UserDialogs.Instance.Toast($"已登记 {student.Name}", new TimeSpan(0, 0, 3));
                        }
                    }
                }
            });

            EditScanLogCommand = new Command<HomeworkStatus>(async (status) =>
            {
                if (colors.Count > 0)
                {
                    var res = await UserDialogs.Instance.ActionSheetAsync(
                        $"{status.Student.Name} 已登记为 {LabelUtils.NameToChineseDisplay(status.Color, homework.Type)}", "好", "删除登记", null,
                        colors.Select(i => LabelUtils.NameToChineseDisplay(i, homework.Type)).ToArray());
                    if (res == "删除登记")
                    {
                        UserDialogs.Instance.Toast($"已删除 {status.Student.Name} 登记的作业", new TimeSpan(0, 0, 3));
                        realm.Write(() => status.HasScanned = false);
                    }
                    else if (!string.IsNullOrEmpty(res) && res != "好")
                    {
                        var color = LabelUtils.ChineseDisplayToName(res);
                        realm.Write(() => status.Color = color);
                        UserDialogs.Instance.Toast($"已将 {status.Student.Name} 的颜色改为 {LabelUtils.NameToChineseDisplay(color, homework.Type)}", new TimeSpan(0, 0, 3));
                    }
                }
                else
                {
                    if (await UserDialogs.Instance.ConfirmAsync($"要删除 {status.Student.Name} 登记的作业吗", $"{status.Student.Name} 已经登记了"))
                    {
                        UserDialogs.Instance.Toast($"已删除 {status.Student.Name} 登记的作业", new TimeSpan(0, 0, 3));
                        realm.Write(() => status.HasScanned = false);
                    }
                }
            });

            EditHomeworkTypeCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(EditHomeworkTypePage)}?typeId={homework.Type.Id}");
            });

            ChangeHomeworkTypeCommand = new Command(async () =>
            {
                var types = realm.All<HomeworkType>().ToList();
                var names = types.Select(x => x.Name).ToArray();
                var res = await UserDialogs.Instance.ActionSheetAsync("选择作业分类", "取消", "", null, names);
                var type = types.Find(i => i.Name == res);
                if (type != null && type != homework.Type)
                {
                    realm.Write(() => homework.Type = type);
                    colors = type.Colors.ToList();
                }
            });
        }

        public IEnumerable<ChartEntry> GetStatsChartEntry()
        {
            return ChartUtils.GetHomeworkStatusPieChartEntries(homework.Status, homework.Type);
        }
    }
}
