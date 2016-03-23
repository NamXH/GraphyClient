using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GraphyClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(DateTime.UtcNow);

//            var t = DoWork();
//            t.Wait();
//            DoWork();

            var db1 = new DatabaseManager("b", 4);
        }

        public static void DoWork()
        {
            var db1 = new DatabaseManager("1", 4);
            var contacts = db1.GetRows<Contact>();

            foreach (var contact in contacts)
            {
                var x = SyncHelper.PostAsync("contacts", contact).Result;
                var b = 1;
            }

//            var result = await SyncHelper.GetAsync<Contact>("contacts");
            var a = 1;

        }
    }
}