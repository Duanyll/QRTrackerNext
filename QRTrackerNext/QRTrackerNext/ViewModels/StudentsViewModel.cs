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
using System.Text;

namespace QRTrackerNext.ViewModels
{
    class StudentsViewModel : BaseViewModel
    {
        private Student selectedStudent;
        public Student SelectedStudent
        {
            get => selectedStudent;
            set
            {
                SetProperty(ref selectedStudent, value);
                OnStudentSelected(value);
            }
        }

        public ObservableCollection<Student> Students { get; }
        public Command LoadStudentsCommand { get; }
        public Command AddStudentCommand { get; }
        public Command ImportStudentCommand { get; }
        public Command ExportStudentCommand { get; }
        public Command<Student> UpdateStudentCommand { get; }
        public Command<Student> RemoveStudentCommand { get; }
        public Command<Student> StudentTapped { get; }

        public Command ShowGroupQrCommand { get; }
        public Command ExportStatsCommand { get; }

        private Realm realm;
        protected Group group;

        private string groupId;
        public string GroupId
        {
            get => groupId;
            set
            {
                SetProperty(ref groupId, value);
            }
        }

        public StudentsViewModel(string groupId)
        {
            GroupId = groupId;
            realm = Services.RealmManager.OpenDefault();
            group = realm.Find<Group>(ObjectId.Parse(groupId));
            Students = new ObservableCollection<Student>();
            if (group == null)
            {
                Title = "班级未找到";
                return;
            }
            Title = group.Name;
            LoadStudentsCommand = new Command(() =>
            {
                IsBusy = true;
                try
                {
                    if (group == null)
                    {
                        return;
                    }
                    Students.Clear();
                    var studentsQuery = group.Students.OrderBy(i => i.Name);
                    foreach (var i in studentsQuery)
                    {
                        Students.Add(i);
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
            AddStudentCommand = new Command(async () =>
            {
                var result = await UserDialogs.Instance.PromptAsync("请输入新建学生名称", "新建学生");
                if (result.Ok)
                {
                    if (string.IsNullOrWhiteSpace(result.Text) || result.Text.Contains(',') || result.Text.Contains(';'))
                    {
                        await UserDialogs.Instance.AlertAsync("请输入有效的名称, 不能包含逗号或分号", "错误");
                        return;
                    }
                    realm.Write(() =>
                    {
                        var student = new Student()
                        {
                            Name = result.Text.Trim()
                        };
                        realm.Add(student);
                        group.Students.Add(student);
                    });
                }
            });
            ImportStudentCommand = new Command(async () =>
            {
                if (Clipboard.HasText)
                {
                    var s = await Clipboard.GetTextAsync();
                    if (s == null) s = await Clipboard.GetTextAsync();
                    if (s == null)
                    {
                        await UserDialogs.Instance.AlertAsync("请确保已授权剪贴板权限, 并再试一次", "读取失败");
                        return;
                    }
                    var str = s.Split(new char[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);

                    var newNames = new List<string>();
                    var existedNames = new List<(string, ObjectId)>();
                    var failedCount = 0;
                    foreach (var i in str)
                    {
                        var vs = i.Split(',');
                        if (!string.IsNullOrWhiteSpace(vs[0]))
                        {
                            if (vs[0].Trim().Length > 15)
                            {
                                if (!await UserDialogs.Instance.ConfirmAsync($"{vs[0].Trim()} 太长了, 这是需要导入的吗?", "导入确认"))
                                {
                                    continue;
                                }
                            }
                            if (vs.Length > 1)
                            {
                                if (ObjectId.TryParse(vs[1].Trim(), out var id) && realm.Find<Student>(id) == null)
                                {
                                    existedNames.Add((vs[0].Trim(), id));
                                }
                                else
                                {
                                    failedCount++;
                                }
                            }
                            else
                            {
                                newNames.Add(vs[0].Trim());
                            }
                        }
                    }
                    if (newNames.Count + existedNames.Count != 0)
                    {
                        var text = new StringBuilder($"识别到 {newNames.Count + existedNames.Count + failedCount} 个学生, ");
                        if (newNames.Count > 0) text.AppendLine($"有 {newNames.Count} 个新学生, ");
                        if (existedNames.Count > 0) text.AppendLine($"有 {existedNames.Count} 个其他设备上创建的学生, 这些学生可以使用原先打印的二维码, ");
                        if (failedCount > 0) text.AppendLine($"有 {failedCount} 个学生已经被导入过了, ");
                        text.AppendLine("要导入这些学生吗?");
                        var result = await UserDialogs.Instance.ConfirmAsync(text.ToString(), "导入学生");
                        if (result)
                        {
                            realm.Write(() =>
                            {
                                foreach (var name in newNames)
                                {
                                    var student = new Student() { Name = name };
                                    realm.Add(student);
                                    group.Students.Add(student);
                                }
                                foreach (var (name, id) in existedNames)
                                {
                                    var student = new Student { Name = name, Id = id };
                                    realm.Add(student);
                                    group.Students.Add(student);
                                }
                            });
                        }
                    }
                    else if (failedCount > 0)
                    {
                        await UserDialogs.Instance.AlertAsync($"这 {failedCount} 个学生都已经导入过了", "导入失败");
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync("请将学生名单复制到剪贴板中, 每行一个姓名, 或者复制导出的学生名单", "导入说明");
                    }
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("请将学生名单复制到剪贴板中, 每行一个姓名, 或者复制导出的学生名单", "导入说明");
                }
            });
            ExportStudentCommand = new Command(async () =>
            {
                var sb = new StringBuilder();
                foreach (var i in group.Students)
                {
                    sb.AppendLine($"{i.Name}, {i.Id};");
                }
                var str = sb.ToString();
                await Clipboard.SetTextAsync(str);
                await UserDialogs.Instance.AlertAsync("已复制到剪贴板, 请发送到其他设备上, 成功导入后即可使用相同的二维码识别学生", "导出成功");
            });
            UpdateStudentCommand = new Command<Student>(async (student) =>
            {
                var result = await UserDialogs.Instance.PromptAsync($"将 {student.Name} 重命名为", "重命名学生");
                if (result.Ok && !string.IsNullOrWhiteSpace(result.Text))
                {
                    if (string.IsNullOrWhiteSpace(result.Text) || result.Text.Contains(',') || result.Text.Contains(';'))
                    {
                        await UserDialogs.Instance.AlertAsync("请输入有效的名称, 不能包含逗号或分号", "错误");
                        return;
                    }
                    realm.Write(() =>
                    {
                        student.Name = result.Text.Trim();
                    });
                }
            });
            RemoveStudentCommand = new Command<Student>(async (student) =>
            {
                var result = await UserDialogs.Instance.ConfirmAsync($"这将同时删除 {student.Name} 的所有作业记录。确定要删除 {student.Name} 吗", "删除学生");
                if (result)
                    realm.Write(() =>
                    {
                        var scanLogs = realm.All<ScanLog>().Where(i => i.Student == student);
                        realm.RemoveRange(scanLogs);
                        realm.Remove(student);
                    });
            });
            StudentTapped = new Command<Student>(OnStudentSelected);
            ShowGroupQrCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(GroupQrPage)}?groupId={groupId}");
            });
            ExportStatsCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(GroupStatsPage)}?groupId={groupId}");
            });
        }

        private IDisposable realmToken;
        public void OnAppearing()
        {
            selectedStudent = null;
            realmToken = group?.Students.SubscribeForNotifications((sender, changes, error) =>
            {
                if (error != null)
                {
                    Debug.WriteLine(error);
                }
                LoadStudentsCommand.Execute(null);
            });
        }

        public void OnDisappearing()
        {
            realmToken?.Dispose();
        }

        async void OnStudentSelected(Student student)
        {
            if (student == null) return;
            await Shell.Current.GoToAsync($"{nameof(StudentDetailPage)}?studentId={student.Id}");
        }
    }
}
