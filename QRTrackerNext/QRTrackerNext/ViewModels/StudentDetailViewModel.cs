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
using TinyPinyin.Core;
using Microcharts;

namespace QRTrackerNext.ViewModels
{
    class StudentDetailViewModel : BaseViewModel
    {
        Realm realm;

        public Student Student { get; }

        public string Uri
        {
            get => QRHelper.GetStudentUri(Student);
        }

        public string UriShort
        {
            get => QRHelper.GetStudentUriShort(Student);
        }

        public IList<string> HomeworkTypeNames { get; }

        int selectedTypeIndex = -1;
        public int SelectedTypeIndex
        {
            get => selectedTypeIndex;
            set
            {
                SetProperty(ref selectedTypeIndex, value);
                if (value != -1)
                {
                    SelectedStatus = typeToStatus[value];
                    ChartEntries = ChartUtils.GetHomeworkStatusPieChartEntries(SelectedStatus.AsQueryable(), typeToStatus[value].Key);
                }
            }
        }

        IEnumerable<HomeworkStatus> selectedStatus = null;
        public IEnumerable<HomeworkStatus> SelectedStatus
        {
            get => selectedStatus;
            set => SetProperty(ref selectedStatus, value);
        }

        IEnumerable<ChartEntry> chartEntries = null;
        public IEnumerable<ChartEntry> ChartEntries
        {
            get => chartEntries;
            set {
                SetProperty(ref chartEntries, value);
                UpdateChart?.Invoke();
            }
        }

        IList<IGrouping<HomeworkType, HomeworkStatus>> typeToStatus;

        public Action UpdateChart { get; set; } 

        public Command RenameStudentCommand { get; }
        public Command RemoveStudentCommand { get; }

        public StudentDetailViewModel(string studentId)
        {
            realm = Services.RealmManager.OpenDefault();
            Student = realm.Find<Student>(ObjectId.Parse(studentId));
            typeToStatus = Student.Homeworks.ToList().GroupBy(i => i.Homework.Type).ToList();
            HomeworkTypeNames = typeToStatus.Select(i => i.Key.Name).ToList();

            RenameStudentCommand = new Command(async () =>
            {
                var result = await UserDialogs.Instance.PromptAsync($"将 {Student.Name} 重命名为", "重命名学生");
                if (result.Ok && !string.IsNullOrWhiteSpace(result.Text))
                {
                    if (string.IsNullOrWhiteSpace(result.Text) || result.Text.Contains(',') || result.Text.Contains(';'))
                    {
                        await UserDialogs.Instance.AlertAsync("请输入有效的名称, 不能包含逗号或分号", "错误");
                        return;
                    }
                    realm.Write(() =>
                    {
                        Student.Name = result.Text.Trim();
                    });
                }
            });

            RemoveStudentCommand = new Command(async () =>
            {
                var result = await UserDialogs.Instance.ConfirmAsync($"这将同时删除 {Student.Name} 的所有作业记录。确定要删除 {Student.Name} 吗", "删除学生");
                if (result)
                {
                    realm.Write(() =>
                    {
                        var status = realm.All<HomeworkStatus>().Where(i => i.Student == Student);
                        realm.RemoveRange(status);
                        realm.Remove(Student);
                    });
                    await Shell.Current.GoToAsync("..");
                }
            });
        }
    }
}
