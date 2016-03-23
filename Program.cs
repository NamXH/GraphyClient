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
//            DoWork();

            var db1 = new DatabaseManager("b");
//            db1.DbConnection.Update(new Contact{ Id = new Guid("b62c41a1-9108-4a97-a2fd-0c380edade5b"), FirstName = "b_Contact_1_new" });
//            var contact = new Contact
//                {
//                    Id = new Guid("92b03b34-55aa-4bbd-ba0f-1ba89e8309fc"),
//                    FirstName = String.Format("test_new"),
//                    LastModified = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
//                };
//            var a = SyncHelper.PutAsync("contacts", "92b03b34-55aa-4bbd-ba0f-1ba89e8309fc", contact, null).Result;
//            var b = a.Item2;
//            var c = JsonConvert.DeserializeObject<Contact>(b);
        }

        public static void DoWork()
        {
            var db1 = new DatabaseManager("1", 4);
            var contacts = db1.GetRows<Contact>();

//            foreach (var contact in contacts)
//            {
//                var x = SyncHelper.PostAsync("contacts", contact).Result;
//                var b = 1;
//            }

//            var result = await SyncHelper.GetAsync<Contact>("contacts");
            var a = 1;

        }
    }
}