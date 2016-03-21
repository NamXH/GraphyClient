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

//            var db1 = new DatabaseManager("2", 10);
//            var db2 = new DatabaseManager("2"); 

//            var contact = new Contact
//            {
//                Id = Guid.NewGuid(),
//                FirstName = "Test04",
//                LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
//            };
//            Task.WaitAll(SyncHelper.PostAsync("contacts", contact));
//            Task.WaitAll(SyncHelper.DeleteAsync("contacts", "8f9d224b-7455-40f3-952e-c5b2640bd34d", new DateTime(2016, 1, 2, 0, 0, 0, DateTimeKind.Utc)));

//            var t = DoWork();
//            t.Wait();
//            DoWork();
            var db = new DatabaseManager("2", 4);
            SyncOperation x = null;
            SyncOperation z = null;

            try
            {
                x = db.DbConnection.Get<SyncOperation>(new Guid("c3627ba4-4bcc-4a91-8fce-98a92ade9807"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); 
            }
            var y = db.DeleteSyncOperation(x.Id);
            try
            {
                z = db.DbConnection.Get<SyncOperation>(new Guid("c3627ba4-4bcc-4a91-8fce-98a92ade9807"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); 
            }
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