using System;
using FilterServiceProvider.Services;
using FilterServiceProvider.Mac.Services;
using System.Threading;
using System.IO;

namespace FilterServiceProvider.Mac
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting FilterServiceProvider.Mac");

            Console.WriteLine($"PlatformID = {Environment.OSVersion.Version}");
            bool quit = false;

            MacPlatformFactory factory = new MacPlatformFactory();
            MacFilterService service = new MacFilterService();
            FilterProvider provider = new FilterProvider(factory, service);

            provider.OnStartup();

            while (!quit)
            {
                Thread.Sleep(100);
            }
        }
    }
}
