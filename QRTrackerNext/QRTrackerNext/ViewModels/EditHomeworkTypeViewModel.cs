using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Realms;
using MongoDB.Bson;
using Acr.UserDialogs;

using Xamarin.Forms;
using TinyPinyin.Core;

using QRTrackerNext.Models;
using QRTrackerNext.Views;

namespace QRTrackerNext.ViewModels
{
    public class ColorConfig : NotifyPropertyChanged
    {
        public string ColorName { get; set; }
        bool isEnabled = false;
        public bool IsEnabled
        {
            get => isEnabled;
            set => SetProperty(ref isEnabled, value);
        }
        string description = "";
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }
    }

    class EditHomeworkTypeViewModel : BaseViewModel
    {
        HomeworkType homeworkType;
        public HomeworkType HomeworkType
        { 
            get => homeworkType;
            set => SetProperty(ref homeworkType, value);
        }

        public IList<ColorConfig> Colors { get; set; }

        string name;
        public string Name { get => name; set => SetProperty(ref name, value);}
        string notCheckedDescription;
        public string NotCheckedDescription
        {
            get => notCheckedDescription;
            set => SetProperty(ref notCheckedDescription, value);
        }
        string noColorDescription;
        public string NoColorDescription
        {
            get => noColorDescription;
            set => SetProperty(ref noColorDescription, value);
        }

        public Command SaveAndExitCommand { get; set; } 

        public EditHomeworkTypeViewModel(string idString)
        {
            var realm = Services.RealmManager.OpenDefault();
            HomeworkType = realm.Find<HomeworkType>(ObjectId.Parse(idString));
            if (HomeworkType == null)
            {
                return;
            }
            Title = HomeworkType.Name;
            Name = HomeworkType.Name;
            NotCheckedDescription = HomeworkType.NotCheckedDescription;
            NoColorDescription = HomeworkType.NoColorDescription;
            var colors = HomeworkType.Colors.ToList();
            Colors = new List<ColorConfig>();
            foreach (var color in LabelUtils.allColors)
            {
                Colors.Add(new ColorConfig()
                {
                    ColorName = color,
                    IsEnabled = colors.Contains(color),
                    Description = HomeworkType.ColorDescriptions.ContainsKey(color) ? HomeworkType.ColorDescriptions[color] : ""
                });
            }

            SaveAndExitCommand = new Command(async () =>
            {
                if (string.IsNullOrEmpty(Name))
                {
                    UserDialogs.Instance.Alert("作业类型不能为空", "保存失败", "确认");
                    return;
                }
                var sameName = realm.All<HomeworkType>().Where(i => i.Name == Name && i.Id != homeworkType.Id).Count();
                if (sameName != 0)
                {
                    UserDialogs.Instance.Alert("已经有相同名称的作业分类了", "保存失败", "确认");
                    return;
                }
                realm.Write(() =>
                {
                    HomeworkType.Name = Name;
                    HomeworkType.NotCheckedDescription = NotCheckedDescription.Trim();
                    HomeworkType.NoColorDescription = NoColorDescription.Trim();
                    HomeworkType.Colors.Clear();
                    foreach (var data in Colors)
                    {
                        if (data.IsEnabled)
                        {
                            HomeworkType.Colors.Add(data.ColorName);
                        }
                        HomeworkType.ColorDescriptions[data.ColorName] = data.Description;
                    }
                });
                await Shell.Current.GoToAsync("..");
            });
        }
    }
}
