using System;
using System.Threading.Tasks;

namespace GraphyClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello");
//            var client = new Client();
//            Task.WaitAll(client.PostTest());

//            var db1 = new DatabaseManager("1", 10);
//            var db2 = new DatabaseManager("2"); 
//            var db3 = new DatabaseManager("3"); 

//            var a = await SyncHelper.GetAsync<Contact>("contacts");
//            Task.WaitAll(SyncHelper.GetAsync<Contact>("contacts"));

            var contact = new Contact
                {   
                    Id = Guid.NewGuid(),
                    FirstName = "Test05",
                    Organization = "Test05",
                    LastModified = DateTime.UtcNow,
                };
            Task.WaitAll(SyncHelper.PostAsync("contacts", contact));
        }
    }
}