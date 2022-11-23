using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Cryptography;

namespace SiloV1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {         
            Console.WriteLine("Silov1 starting");
            var ipcServer = new LocalCommandServer();
            await ipcServer.StartAsync();
            Console.WriteLine("Silov1 started");
            await ipcServer.Completed;
            Console.WriteLine("Silov1 completed");
        }        
    }
}