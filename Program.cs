using System;
using System.Threading.Tasks;

namespace GraphyClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(DateTime.UtcNow);
//            var client = new Client();
//            Task.WaitAll(client.PostTest());

//            var db1 = new DatabaseManager("2", 10);
//            var db2 = new DatabaseManager("2"); 
//            var db3 = new DatabaseManager("3"); 

//            var a = await SyncHelper.GetAsync<Contact>("contacts");
//            Task.WaitAll(SyncHelper.GetAsync<Contact>("contacts"));

            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                FirstName = "Test04",
                LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
            };
//            Task.WaitAll(SyncHelper.PostAsync("contacts", contact));

            Task.WaitAll(SyncHelper.DeleteAsync("contacts", "8f9d224b-7455-40f3-952e-c5b2640bd34d", new DateTime(2016, 1, 2, 0, 0, 0, DateTimeKind.Utc)));
        }
    }
}