namespace SiloV2
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Silov2 starting");
            var ipcServer = new LocalCommandServer();
            await ipcServer.StartAsync();
            Console.WriteLine("Silov2 started");
            await ipcServer.Completed;
            Console.WriteLine("Silov2 completed");
        }
    }
}