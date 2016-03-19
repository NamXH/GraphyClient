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

            var db1 = new DatabaseManager("2", 300);
//            var db2 = new DatabaseManager("2"); 
//            var db3 = new DatabaseManager("3"); 
        }
    }
}