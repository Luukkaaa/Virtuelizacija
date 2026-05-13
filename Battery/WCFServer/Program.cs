using System;
using System.ServiceModel;

namespace WCFServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(EisService)))
            {
                host.Open();
                Console.WriteLine("WCF Servis je uspešno pokrenut na net.tcp://localhost:8000/EisService");
                Console.WriteLine("Pritisnite ENTER za gašenje...");
                Console.ReadLine();
            }
        }
    }
}