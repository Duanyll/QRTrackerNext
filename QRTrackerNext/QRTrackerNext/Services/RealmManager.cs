using System;
using System.Collections.Generic;
using System.Text;

using Realms;
using MongoDB.Bson;

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
                Console.WriteLine("Migration to version 1: nothing has to be done.");
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

                    foreach (var scanLog in oldHomework.ScanLog)
                    {
                        if (statusIndex.TryGetValue(scanLog.Student.Id, out HomeworkStatus status))
                        {
                            status.Color = scanLog.Color;
                            status.Time = scanLog.Time;
                            status.HasScanned = true;
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
                MigrationCallback = RealmMigrationCallback,
#if DEBUG
                ShouldDeleteIfMigrationNeeded = true
#endif
            });
        }
    }
}
