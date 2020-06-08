using System;
using System.Linq.Expressions;
using SchedulerSystem;

namespace ScheduleSystemTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Tests");
            var names = SchedulerSystem.SchedulerSystem.Instance.GetAllJobNames();


            

            TestStartCancelOne();
            TestStartCancelTwo();


            TestStartNotExist();




            
        }

        private static void TestStartNotExist()
        {
            try
            {
                SchedulerSystem.SchedulerSystem.Instance.RunNewJob("Not a job name!");
            }
            catch(Exception ex)
            {
                return;
            }
            Console.WriteLine("Fail" + nameof(TestStartNotExist));
        }

        private static void TestStartCancelTwo()
        {
            if (!SchedulerSystem.SchedulerSystem.Instance.RunNewJob("Important Task"))
            {
                Console.WriteLine("Fail!");
            }

            if (!SchedulerSystem.SchedulerSystem.Instance.RunNewJob("Scheduled Task"))
            {
                Console.WriteLine("Fail!");
            }

            if (!SchedulerSystem.SchedulerSystem.Instance.StopJob("Scheduled Task"))
            {
                Console.WriteLine("Fail!");
            }

            if (!SchedulerSystem.SchedulerSystem.Instance.StopJob("Important Task"))
            {
                Console.WriteLine("Fail!");
            }

            
        }

        private static void TestStartCancelOne()
        {
            if (!SchedulerSystem.SchedulerSystem.Instance.RunNewJob("Important Task"))
            {
                Console.WriteLine("Fail!");
            }

            if (!SchedulerSystem.SchedulerSystem.Instance.StopJob("Important Task"))
            {
                Console.WriteLine("Fail!");
            }
        }

        

       
    }

}
