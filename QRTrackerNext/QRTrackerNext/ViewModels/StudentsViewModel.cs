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
        public Command ImportExistedStudentCommand { get; }
        public Command ExportStudentCommand { get; }
        public Command<Student> UpdateStudentCommand { get; }
        public Command<Student> RemoveStudentCommand { get; }
        public Command<Student> StudentTapped { get; }

        public Command ShowGroupQrCommand { get; }

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
            realm = Realm.GetInstance();
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
                        await UserDialogs.Instance.AlertAsync("请确保已授权剪贴板权限", "读取失败");
                        return;
                    }
                    var str = s.Split('\n');

                    List<string> names = new List<string>();
                    foreach (var i in str)
                    {
                        if (!string.IsNullOrWhiteSpace(i) && !i.Contains(',') && !i.Contains(';'))
                        {
                            if (i.Contains(','))
                            {
                                await UserDialogs.Instance.AlertAsync("不能导入含有逗号的名称", "导入失败");
                                return;
                            }
                            if (i.Trim().Length > 15)
                            {
                                if (!await UserDialogs.Instance.ConfirmAsync($"{i} 看起来太长了. 这是一个要导入的名称吗", "提示"))
                                {
                                    continue;
                                }
                            }
                            names.Add(i.Trim());
                        }
                    }
                    if (names.Count != 0)
                    {
                        var result = await UserDialogs.Instance.ConfirmAsync($"要导入 {names.Count} 个学生吗", "导入学生");
                        if (result)
                        {
                            realm.Write(() =>
                            {
                                foreach (var name in names)
                                {
                                    var student = new Student() { Name = name };
                                    realm.Add(student);
                                    group.Students.Add(student);
                                }
                            });
                        }
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync("请将学生名单复制到剪贴板中, 每行一个姓名", "导入说明");
                    }
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("请将学生名单复制到剪贴板中, 每行一个姓名", "导入说明");
                }
            });
            ImportExistedStudentCommand = new Command(async () =>
            {
                if (Clipboard.HasText)
                {
                    var s = await Clipboard.GetTextAsync();
                    if (s == null) s = await Clipboard.GetTextAsync();
                    if (s == null)
                    {
                        await UserDialogs.Instance.AlertAsync("请确保已授权剪贴板权限", "读取失败");
                        return;
                    }
                    var str = s.Split(';');
                    
                    var names = new List<(string name, ObjectId id)>();
                    foreach (var i in str)
                    {
                        if (!string.IsNullOrWhiteSpace(i))
                        {
                            var vs = i.Split(',');
                            if (vs.Length == 2 && !string.IsNullOrWhiteSpace(vs[0]) && ObjectId.TryParse(vs[1].Trim(), out var id))
                            {
                                if (realm.Find<Student>(id) != null) { continue; }
                                names.Add((vs[0].Trim(), id));
                            }
                        }
                    }
                    if (names.Count != 0)
                    {
                        var result = await UserDialogs.Instance.ConfirmAsync($"要导入 {names.Count} 个学生吗", "导入学生");
                        if (result)
                        {
                            realm.Write(() =>
                            {
                                foreach (var (name, id) in names)
                                {
                                    var student = new Student() { Name = name, Id = id };
                                    realm.Add(student);
                                    group.Students.Add(student);
                                }
                            });
                        }
                    }
                    else
                    {
                        await UserDialogs.Instance.AlertAsync("没有找到可以导入的学生. 有的学生可能已经被导入过了", "导入失败");
                    }
                }
                else
                {
                    await UserDialogs.Instance.AlertAsync("请将 \"导出学生\" 选项生成的内容复制到剪贴板中", "导入说明");
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
                await UserDialogs.Instance.AlertAsync("已复制到剪贴板, 在其他设备上使用 \"导入其他设备上的学生\" 选项来导入", "导出成功");
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
                        var scanLogs = realm.All<ScanLog>().Where(i => i.student == student);
                        realm.RemoveRange(scanLogs);
                        realm.Remove(student);
                    });
            });
            StudentTapped = new Command<Student>(OnStudentSelected);
            ShowGroupQrCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{nameof(GroupQrPage)}?groupId={groupId}");
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
