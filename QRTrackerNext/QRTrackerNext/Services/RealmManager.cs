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
                // Process Pinyin
                foreach (var student in migration.NewRealm.DynamicApi.All("Studnet"))
                {
                    student.NamePinyin = PinyinHelper.GetPinyin(student.Name);
                }
                foreach (var group in migration.NewRealm.DynamicApi.All("Group"))
                {
                    group.NamePinyin = PinyinHelper.GetPinyin(group.Name);
                }

                // group to student => student to group
                foreach (var oldGroup in migration.OldRealm.DynamicApi.All("Group"))
                {
                    var groupId = (ObjectId)oldGroup._id;
                    var newGroup = migration.NewRealm.Find<Group>(groupId);
                    foreach (var oldStudent in oldGroup.Students)
                    {
                        var studentId = (ObjectId)oldStudent._id;
                        var newStudent = migration.NewRealm.Find<Student>(studentId);
                        newStudent.Group = newGroup;
                    }
                }

                // create the default homework type
                var defaultType = migration.NewRealm.Add(GetDefaultHomeworkType());

                // ScanLog => HomeworkStatus
                foreach (var newHomework in migration.NewRealm.All<Homework>())
                {
                    newHomework.Type = defaultType;
                    var homeworkId = newHomework.Id;
                    var oldHomework = migration.OldRealm.DynamicApi.Find("Homework", homeworkId);
                    var statusMap = new Dictionary<ObjectId, HomeworkStatus>();
                    foreach (var oldGroup in oldHomework.Groups)
                    {
                        foreach (var oldStudent in oldGroup.Students)
                        {
                            var studentId = (ObjectId)oldStudent._id;
                            var newStudent = migration.NewRealm.Find<Student>(studentId);
                            var status = new HomeworkStatus()
                            {
                                Student = newStudent,
                                Time = oldHomework.CreationTime,
                                Color = "gray",
                                Homework = newHomework,
                                HasScanned = false
                            };
                            migration.NewRealm.Add(status);
                            statusMap.Add(studentId, status);
                        }
                    }
                    foreach (var scanLog in oldHomework.Scans)
                    {
                        var status = statusMap[(ObjectId)scanLog.Student._id];
                        status.HasScanned = true;
                        status.Time = scanLog.Time;
                        status.Color = scanLog.Color ?? "gray";
                        if (status.Color == "grey")
                        {
                            status.Color = "gray";
                        }
                    }
                }
            }
        }

        public static Realm OpenDefault()
        {
            return Realm.GetInstance(new RealmConfiguration()
            {
                SchemaVersion = 2,
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
