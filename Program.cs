using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GraphyClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var t = DoWork();
            t.Wait();
        }

        public static async Task DoWork()
        {
            // Create db1
//            var db1 = new DatabaseManager("x", 10);
//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await db1.SyncDatabaseAsync();
//            sw.Stop();
//            Console.WriteLine("Elapsed={0}", sw.Elapsed);

            // Make changes db1
//            var db1 = new DatabaseManager("x");
//            db1.MakeChanges("x_new1", 2, new DateTime(2016, 1, 2, 0, 0, 0, DateTimeKind.Utc));
//            await db1.SyncDatabaseAsync();

            // Pull db2
//            var db2 = new DatabaseManager("y", 0);
//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await db2.SyncDatabaseAsync();
//            sw.Stop();
//            Console.WriteLine("Elapsed={0}", sw.Elapsed);

            // Make changes db2
//            var db2 = new DatabaseManager("y");
//            db2.MakeChanges("y_new2", 2, new DateTime(2016, 1, 3, 0, 0, 0, DateTimeKind.Utc));
//            await db2.SyncDatabaseAsync();

            // Test interrupted sync & conflicts
//            var db1 = new DatabaseManager("x");
//            db1.MakeChangesToSameElements("_same1", 2, new DateTime(2016, 1, 3, 12, 0, 0, DateTimeKind.Utc));
//            var db2 = new DatabaseManager("y");
//            db2.MakeChangesToSameElements("_same2", 2, new DateTime(2016, 1, 4, 12, 0, 0, DateTimeKind.Utc));
//            await db1.SyncDatabaseAsync();
//            await db2.SyncDatabaseAsync();

            // Massive sync
//            var dbm1 = new DatabaseManager("x1", 0);
//            await dbm1.SyncDatabaseAsync();
//            var dbm2 = new DatabaseManager("x2", 0);
//            await dbm2.SyncDatabaseAsync();
//            var dbm3 = new DatabaseManager("x3", 0);
//            await dbm3.SyncDatabaseAsync();
//            var dbm4 = new DatabaseManager("x4", 0);
//            await dbm4.SyncDatabaseAsync();
//            var dbm5 = new DatabaseManager("x5", 0);
//            await dbm5.SyncDatabaseAsync();
//            var dbm6 = new DatabaseManager("x6", 0);
//            await dbm6.SyncDatabaseAsync();
//            var dbm7 = new DatabaseManager("x7", 0);
//            await dbm7.SyncDatabaseAsync();
//            var dbm8 = new DatabaseManager("x8", 0);
//            await dbm8.SyncDatabaseAsync();
//            var dbm9 = new DatabaseManager("x9", 0);
//            await dbm9.SyncDatabaseAsync();

//            var dbm1 = new DatabaseManager("x1");
//            dbm1.MakeChangesToSameElements("_same1", 2, new DateTime(2016, 1, 2, 12, 0, 0, DateTimeKind.Utc));
//            var dbm2 = new DatabaseManager("x2");
//            dbm2.MakeChangesToSameElements("_same2", 2, new DateTime(2016, 1, 3, 12, 0, 0, DateTimeKind.Utc));
//            var dbm3 = new DatabaseManager("x3");
//            dbm3.MakeChangesToSameElements("_same3", 2, new DateTime(2016, 1, 4, 12, 0, 0, DateTimeKind.Utc));
//            var dbm4 = new DatabaseManager("x4");
//            dbm4.MakeChangesToSameElements("_same4", 2, new DateTime(2016, 1, 5, 12, 0, 0, DateTimeKind.Utc));
//            var dbm5 = new DatabaseManager("x5");
//            dbm5.MakeChangesToSameElements("_same5", 2, new DateTime(2016, 1, 6, 12, 0, 0, DateTimeKind.Utc));
//            var dbm6 = new DatabaseManager("x6");
//            dbm6.MakeChangesToSameElements("_same6", 2, new DateTime(2016, 1, 7, 12, 0, 0, DateTimeKind.Utc));
//            var dbm7 = new DatabaseManager("x7");
//            dbm7.MakeChangesToSameElements("_same7", 2, new DateTime(2016, 1, 8, 12, 0, 0, DateTimeKind.Utc));
//            var dbm8 = new DatabaseManager("x8");
//            dbm8.MakeChangesToSameElements("_same8", 2, new DateTime(2016, 1, 9, 12, 0, 0, DateTimeKind.Utc));
//            var dbm9 = new DatabaseManager("x9");
//            dbm9.MakeChangesToSameElements("_same9", 2, new DateTime(2016, 1, 10, 12, 0, 0, DateTimeKind.Utc));

//            var db1 = new DatabaseManager("x");
//            var dbm1 = new DatabaseManager("x1");
//            var dbm2 = new DatabaseManager("x2");
//            var dbm3 = new DatabaseManager("x3");
//            var dbm4 = new DatabaseManager("x4");
//            var dbm5 = new DatabaseManager("x5");
//            var dbm6 = new DatabaseManager("x6");
//            var dbm7 = new DatabaseManager("x7");
//            var dbm8 = new DatabaseManager("x8");
//            var dbm9 = new DatabaseManager("x9");
//
//            var tasks = new List<Task>();
//            tasks.Add(db1.SyncDatabaseAsync());
//            tasks.Add(dbm1.SyncDatabaseAsync());
//            tasks.Add(dbm2.SyncDatabaseAsync());
//            tasks.Add(dbm3.SyncDatabaseAsync());
//            tasks.Add(dbm4.SyncDatabaseAsync());
//            tasks.Add(dbm5.SyncDatabaseAsync());
//            tasks.Add(dbm6.SyncDatabaseAsync());
//            tasks.Add(dbm7.SyncDatabaseAsync());
//            tasks.Add(dbm8.SyncDatabaseAsync());
//            tasks.Add(dbm9.SyncDatabaseAsync());
//            await Task.WhenAll(tasks);
        }
    }
}