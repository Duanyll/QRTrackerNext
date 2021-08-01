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
                if (result.Ok && !string.IsNullOrWhiteSpace(result.Text))
                {
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
                    var str = (await Clipboard.GetTextAsync()).Split('\n');
                    List<string> names = new List<string>();
                    foreach (var i in str)
                    {
                        if (!string.IsNullOrWhiteSpace(i))
                        {
                            names.Add(i);
                        }
                    }
                    var result = await UserDialogs.Instance.ConfirmAsync($"要导入 ${names.Count} 个学生吗", "导入学生");
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
            });
            UpdateStudentCommand = new Command<Student>(async (student) =>
            {
                var result = await UserDialogs.Instance.PromptAsync($"将 {student.Name} 重命名为", "重命名学生");
                if (result.Ok && !string.IsNullOrWhiteSpace(result.Text))
                {
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
