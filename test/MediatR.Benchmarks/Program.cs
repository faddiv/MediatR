using BenchmarkDotNet.Running;
using System.Threading.Tasks;

namespace MediatR.Benchmarks
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            ///*
            Benchmarks b = new Benchmarks();
            b.GlobalSetup();
            await b.SendingRequests();
            await b.PublishingNotifications();/**/
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}