using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GraphyClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(DateTime.UtcNow);

//            var t = DoWork();
//            t.Wait();

            var db1 = new DatabaseManager("x", 8);
            db1.MakeChanges("x_new1", 2, DateTime.UtcNow);
        }

        public static async Task DoWork()
        {
            var db1 = new DatabaseManager("x", 8);
//            await db1.SyncDatabaseAsync();

//            var db2 = new DatabaseManager("y", 0);
//            await db2.SyncDatabaseAsync();

//            var db1 = new DatabaseManager("x");

            db1.MakeChanges("x_new1", 2, DateTime.UtcNow);
//            await db1.SyncDatabaseAsync();
        }
    }
}