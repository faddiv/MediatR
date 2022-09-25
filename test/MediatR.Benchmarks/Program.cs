using BenchmarkDotNet.Running;
using System.Threading.Tasks;

namespace MediatR.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {

            
            /*Benchmarks b = new Benchmarks();
            b.GlobalSetup();
            b.SendingRequests().Wait();
            //await b.PublishingNotifications();/**/
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}