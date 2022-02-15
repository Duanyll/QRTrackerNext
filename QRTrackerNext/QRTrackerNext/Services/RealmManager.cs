using System;
using System.Collections.Generic;
using System.Text;

using Realms;
using MongoDB.Bson;
using TinyPinyin.Core;

using QRTrackerNext.Models;
using System.Linq;
using Xamarin.Essentials;

namespace QRTrackerNext.Services
{
    internal class RealmManager
    {
        static HomeworkType GetDefaultHomeworkType()
        {
            var type = new HomeworkType() { Name = "默认分类", IsBuiltin = true };
            type.Colors.Add("red");
            type.Colors.Add("yellow");
            type.Colors.Add("green");
            type.Colors.Add("blue");
            type.Colors.Add("purple");
            type.ColorDescriptions.Add("red", Preferences.Get("name_red", ""));
            type.ColorDescriptions.Add("yellow", Preferences.Get("name_yellow", ""));
            type.ColorDescriptions.Add("green", Preferences.Get("name_green", ""));
            type.ColorDescriptions.Add("blue", Preferences.Get("name_blue", ""));
            type.ColorDescriptions.Add("purple", Preferences.Get("name_purple", ""));
            return type;
        }

        static void RealmMigrationCallback(Migration migration, ulong oldSchemaVersion)
        {
            if (oldSchemaVersion < 1)
            {
                Console.WriteLine("Migrate to version 1: nothing has to be done.");
            }
            if (oldSchemaVersion < 2)
            {
                Console.WriteLine("Migrate to version 2: Rearrange Homework status.");
                var oldHomeworks = migration.OldRealm.DynamicApi.All("Homework");
                var newHomeworks = migration.NewRealm.All<Homework>();

                for (int i = 0; i < newHomeworks.Count(); i++)
                {
                    var oldHomework = oldHomeworks.ElementAt(i);
                    var newHomework = newHomeworks.ElementAt(i);

                    var students = newHomework.Groups.SelectMany(g => g.Students);
                    var statusIndex = new Dictionary<ObjectId, HomeworkStatus>();
                    foreach (var student in students)
                    {
                        var status = migration.NewRealm.Add(new HomeworkStatus()
                        {
                            Student = student,
                            Time = newHomework.CreationTime,
                            Color = "gray",
                            HomeworkId = newHomework.Id,
                            HasScanned = false,
                        });
                        newHomework.Status.Add(status);
                        statusIndex.Add(student.Id, status);
                    }

                    foreach (var scanLog in oldHomework.Scans)
                    {
                        if (statusIndex.TryGetValue(scanLog.Student._id, out HomeworkStatus status))
                        {
                            status.Color = scanLog.Color;
                            status.Time = scanLog.Time;
                            status.HasScanned = true;
                        }
                    }
                }
            }
            if (oldSchemaVersion < 3)
            {
                Console.WriteLine("Migrate to version 3: Add Student to Group link.");
                var groups = migration.NewRealm.All<Group>();
                foreach (var group in groups)
                {
                    foreach (var student in group.Students)
                    {
                        student.GroupId = group.Id;
                    }
                }
            }
            if (oldSchemaVersion < 4)
            {
                Console.WriteLine("Migrate to version 4: Add Pinyin sorting for Student and Group");
                foreach (var student in migration.NewRealm.All<Student>())
                {
                    student.NamePinyin = PinyinHelper.GetPinyin(student.Name);
                }
                foreach (var group in migration.NewRealm.All<Group>())
                {
                    group.NamePinyin = PinyinHelper.GetPinyin(group.Name);
                }
            }
            if (oldSchemaVersion < 5)
            {
                Console.WriteLine("Migrate to version 5: Add HomeworkType, fix grey typo");
                foreach (var status in migration.NewRealm.All<HomeworkStatus>())
                {
                    if (status.Color == "grey")
                    {
                        status.Color = "gray";
                    }
                }
                var defaultType = GetDefaultHomeworkType();
                migration.NewRealm.Add(defaultType);

                foreach (var homework in migration.NewRealm.All<Homework>())
                {
                    homework.Type = defaultType;
                }
            }
        }

        public static Realm OpenDefault()
        {
            return Realm.GetInstance(new RealmConfiguration()
            {
                SchemaVersion = 5,
                MigrationCallback = RealmMigrationCallback
            });
        }

        public static void InitializeData()
        {
            var realm = OpenDefault();
            if (realm.All<HomeworkType>().Count() < 1)
            {
                realm.Write(() =>
                {
                    realm.Add(GetDefaultHomeworkType());
                });
            }
        }
    }
}
