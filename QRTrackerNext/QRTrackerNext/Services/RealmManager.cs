using System;
using System.Collections.Generic;
using System.Text;

using Realms;

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
        }

        public static Realm OpenDefault()
        {
            return Realm.GetInstance(new RealmConfiguration()
            {
                SchemaVersion = 1,
                MigrationCallback = RealmMigrationCallback,
#if DEBUG
                ShouldDeleteIfMigrationNeeded = true
#endif
            });
        }
    }
}
