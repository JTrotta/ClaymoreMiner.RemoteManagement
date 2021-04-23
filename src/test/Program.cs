using ClaymoreMiner.RemoteManagement;
using System;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var client = new RemoteManagementClient("localhost");
            var a  = client.GetStatisticsAsync();

            var t = a.Result.Gpus[0].Temperature;
            var p = a.Result.Gpus[0].Power;
            Console.WriteLine(t.ToString() +" ** "+ p.ToString());
        }
    }
}
