using System;

namespace SchedulerSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Schedule system boot up....");


            var names = SchedulerSystem.Instance.GetAllJobNames();


            Console.WriteLine("Current tasks are:");
            foreach(var taskName in names)
            {
                Console.WriteLine(taskName);
            }

           

            while(true)
            { 
                Console.WriteLine(@"Please enter ""start {task name}"" task name to start a task or ""stop {task name}"" to end one");
                Console.WriteLine(@"Enter ""e"" to exit");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    return;
                if (input == "e")
                    break;

                if (input.Trim().StartsWith("start"))
                    StartJob(input.Trim());
                else if (input.Trim().StartsWith("stop"))
                    StopJob(input.Trim());
                else
                    Console.WriteLine("Error please enter a valid option!");
            }
        }

        private static void StartJob(string input)
        {
            string name = "";
            try
            {
                name = input.Replace("start", "").Trim();
                
                   
                    if(SchedulerSystem.Instance.RunNewJob(name))
                    {
                        Console.WriteLine($"Job {name} has been started, enter to end it ");
                    }
                    
                    
                    
                
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error starting job {name}: " + ex.Message);
            }


        }

        private static void StopJob(string input)
        {
            string name = "";
            try
            {
                name = input.Replace("stop", "").Trim();
               

                    if (SchedulerSystem.Instance.StopJob(name))
                    {
                        Console.WriteLine($"Job {name} has been stoped!");
                    }



                
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error starting job {name}: " + ex.Message);
            }
        }
    }
}
