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

        public IQueryable<HomeworkStatus> SubmittedStatus { get; set; }
        public IQueryable<HomeworkStatus> NotSubmittedStatus { get; set; }

        public Command GoScanCommand { get; }
        public Command SearchStudentCommand { get; }
        public Command<Student> SubmitStudentCommand { get; }
        public Command<HomeworkStatus> EditScanLogCommand { get; }

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

            var statusIndex = new Dictionary<ObjectId, HomeworkStatus>();
            var allStudents = new List<Student>();
            foreach (var status in homework.Status)
            {
                if (status.Student == null) continue;
                statusIndex.Add(status.Student.Id, status);
                allStudents.Add(status.Student);
            }
            StudentCount = allStudents.Count;
            SubmittedStatus = realm.All<HomeworkStatus>().Where(i => i.HomeworkId == homework.Id && i.HasScanned);
            NotSubmittedStatus = realm.All<HomeworkStatus>().Where(i => i.HomeworkId == homework.Id && !i.HasScanned);
            var colors = homework.Colors.ToList();
            var convertor = new ColorChineseNameConvertor();

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
                                UserDialogs.Instance.Toast($"已将 {status.Student.Name} 登记为 {convertor.Convert(color, null, null, null)}", new TimeSpan(0, 0, 3));
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
                            var color = convertor.ConvertBack(
                                    await UserDialogs.Instance.ActionSheetAsync("选择登记颜色", "不标记颜色", null, null,
                                        colors.Select(i => convertor.Convert(i, null, null, null) as string).ToArray()),
                                    null, null, null) as string;
                            realm.Write(() =>
                            {
                                status.Color = color;
                            });
                            UserDialogs.Instance.Toast($"已登记 {student.Name}, 颜色为 { convertor.Convert(color, null, null, null) }", new TimeSpan(0, 0, 3));
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
                        $"{status.Student.Name} 已登记为 {convertor.Convert(status.Color, null, null, null)}", "好", "删除登记", null,
                        colors.Select(i => convertor.Convert(i, null, null, null) as string).ToArray());
                    if (res == "删除登记")
                    {
                        UserDialogs.Instance.Toast($"已删除 {status.Student.Name} 登记的作业", new TimeSpan(0, 0, 3));
                        realm.Write(() => status.HasScanned = false);
                    }
                    else if (!string.IsNullOrEmpty(res) && res != "好")
                    {
                        var color = convertor.ConvertBack(res, null, null, null) as string;
                        realm.Write(() => status.Color = color);
                        UserDialogs.Instance.Toast($"已将 {status.Student.Name} 的颜色改为 { convertor.Convert(color, null, null, null) }", new TimeSpan(0, 0, 3));
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
        }

        public IEnumerable<ChartEntry> GetStatsChartEntry()
        {
            int submittedCount = SubmittedStatus.Count();
            int notSubmittedCount = NotSubmittedStatus.Count();
            if (homework.Colors.Count == 0)
            {
                return new ChartEntry[]
                {
                    new ChartEntry(submittedCount)
                    {
                        Label = "已登记",
                        ValueLabel = "green",
                        Color = SkiaSharp.SKColors.LimeGreen
                    },
                    new ChartEntry(notSubmittedCount)
                    {
                        Label = "未登记",
                        ValueLabel = "grey",
                        Color = SkiaSharp.SKColors.LightGray
                    }
                };
            }
            else
            {
                var colorCount = new Dictionary<string, int>()
                {
                    {"grey", 0 },
                    {"green" , 0 },
                    {"yellow", 0 },
                    {"red", 0 },
                    {"blue", 0 },
                    {"purple", 0 }
                };
                foreach (var status in SubmittedStatus)
                {
                    colorCount[status.Color]++;
                }
                var res = new List<ChartEntry>()
                {
                    new ChartEntry(notSubmittedCount)
                    {
                        Label = "未登记",
                        ValueLabel = "grey",
                        Color = SkiaSharp.SKColors.LightGray
                    }
                };
                if (colorCount["grey"] > 0)
                {
                    res.Add(new ChartEntry(colorCount["grey"]) {
                        Label = "未标记颜色",
                        ValueLabel = "grey",
                        Color = SkiaSharp.SKColors.Gray
                    });
                }
                var col = new StringToAccentColorConvertor();
                var name = new ColorChineseNameConvertor();
                foreach (var color in homework.Colors)
                {
                    res.Add(new ChartEntry(colorCount[color])
                    {
                        Label = (string)name.Convert(color, null, null, null),
                        ValueLabel = color,
                        Color = SkiaSharp.SKColor.Parse(((Color)col.Convert(color, null, null, null)).ToHex())
                    }); 
                }
                return res;
            }
        }
    }
}
