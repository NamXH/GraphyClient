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
//            var db1 = new DatabaseManager("x", 150);
//            var db1 = new DatabaseManager("x");
//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            await db1.SyncDatabaseAsync();
//            sw.Stop();
//            Console.WriteLine("Elapsed={0}", sw.Elapsed);


//            var db1 = new DatabaseManager("x");
//            db1.MakeChanges("x_new1", 2, new DateTime(2016, 1, 2, 0, 0, 0, DateTimeKind.Utc));
//            await db1.SyncDatabaseAsync();

            var db2 = new DatabaseManager("y7", 0);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            await db2.SyncDatabaseAsync();
            sw.Stop();
            Console.WriteLine("Elapsed={0}", sw.Elapsed);

//            var db2 = new DatabaseManager("y");
//            db2.MakeChanges("y_new2", 2, new DateTime(2016, 1, 3, 0, 0, 0, DateTimeKind.Utc));
//            await db2.SyncDatabaseAsync();

//            var db1 = new DatabaseManager("x");
//            await db1.SyncDatabaseAsync();
        }
    }
}