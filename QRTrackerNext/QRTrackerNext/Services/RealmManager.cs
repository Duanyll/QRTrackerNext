using System;
using System.Collections.Generic;
using System.Text;

using Realms;
using MongoDB.Bson;
using TinyPinyin.Core;

using QRTrackerNext.Models;
using System.Linq;

namespace QRTrackerNext.Services
{
    internal class RealmManager
    {
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
                            Color = "grey",
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
        }

        public static Realm OpenDefault()
        {
            return Realm.GetInstance(new RealmConfiguration()
            {
                SchemaVersion = 4,
                MigrationCallback = RealmMigrationCallback
            });
        }
    }
}
