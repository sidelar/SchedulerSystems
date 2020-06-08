using System;
using System.Threading;

namespace BasicAwaiter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Waits 1 minute and exits!");
            WaitOneMinute();
            
        }

        private static void WaitOneMinute()
        {
            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
    }
}
