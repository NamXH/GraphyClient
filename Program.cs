using System;
using System.Threading.Tasks;

namespace GraphyClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var client = new Client();
            Task.WaitAll(client.PostTest());
        }
    }
}