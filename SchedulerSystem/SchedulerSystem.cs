using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Threading.Tasks;
using System.Diagnostics;


namespace SchedulerSystem
{
    public sealed class SchedulerSystem 
    {

        public static readonly Lazy<SchedulerSystem> _lazy = new Lazy<SchedulerSystem>(() => new SchedulerSystem());
        public static SchedulerSystem Instance => _lazy.Value;


        private ConcurrentDictionary<string, Job> _jobDictionary = new ConcurrentDictionary<string, Job>();
        private readonly object locker = new object();

        private SchedulerSystem()
        {
            LoadConfigFile();


            timer = new System.Timers.Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();



        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            IEnumerable<Job> jobsToStart = null;
            DateTime now = DateTime.Now;
            lock(locker)
            {
              
                if(_jobDictionary.Any(x=> !x.Value.IsRunning && x.Value.NextExecutionTime != null && x.Value.NextExecutionTime < now))
                {
                    jobsToStart =  _jobDictionary.Where(x => !x.Value.IsRunning && x.Value.NextExecutionTime != null && x.Value.NextExecutionTime < now).Select(x => x.Value);
                   

                }
            }
           
            if(jobsToStart != null && jobsToStart.Any())
                StartJobs(jobsToStart);
        }

        private void StartJobs(IEnumerable<Job> jobsToStart)
        {

            foreach (var job in jobsToStart)
            {
                var filePath = job.TaskLocation;
                if (!File.Exists(job.TaskLocation)) // check for file location
                {
                    filePath = Path.Combine(Environment.CurrentDirectory, job.TaskLocation);
                    if (File.Exists(filePath)) // if its not a path, check if we provided it with the output
                    {
                        throw new FileNotFoundException($"Couldnt find file for task {job.TaskName} @ {job.TaskLocation}!");
                    }                 
                       
                }

                Console.WriteLine($"Running Job {job.TaskName} @ {DateTime.Now} ");
                job.CancelationTokenSource = new CancellationTokenSource();

                lock (locker)
                    if (_jobDictionary[job.TaskName].IsRunning)
                        return;
                 Task.Run( async () =>
                {

                    job.CancelationTokenSource.Token.ThrowIfCancellationRequested();
                    
                    Process pr = new Process()
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = filePath,
                            
                        }
                        
                    };

                    await RunAsync(pr, job.CancelationTokenSource.Token);                    
                    Console.WriteLine($"Job {job.TaskName} exited normaly!");
                    StopJob(job.TaskName);



                }, job.CancelationTokenSource.Token);


                job.IsRunning = true;

                UpdateJob(job);
               

                
                    
                
            }
                
        }

        public static Task RunAsync(Process process, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();
            
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(null);
            cancellationToken.Register(() => { 
                tcs.TrySetCanceled(); 
                Console.WriteLine("Canceling job early!!!!");
            });
            if (!process.Start()) tcs.SetException(new Exception("Failed to start process."));
            return tcs.Task;
        }

        public bool StopJob(string name)
        {
            Job job = null;
            lock (locker)
            {
                if (!_jobDictionary.ContainsKey(name))
                    throw new Exception($"Job {name} does not exist!");

                job = _jobDictionary[name];
                if (!job.IsRunning)
                    throw new Exception($"Job {name} is not runing now!");



            }


            CancelJob(job);

            if (job != null)
                return true;


            return false;
        }

        private void UpdateJob(Job job)
        {
            if (job.TaskInterval > 0)
            {
                if (job.NextExecutionTime != null)
                    job.NextExecutionTime = job.NextExecutionTime.Value.AddMinutes(job.TaskInterval);
                else
                    job.NextExecutionTime = DateTime.Now.AddMinutes(job.TaskInterval);
            }
            else if (job.TaskScheduledTme != null)
            {
                job.NextExecutionTime = DateTime.Today.AddDays(1).Add(job.TaskScheduledTme.Value.TimeOfDay);
            }


            lock (locker)
                _jobDictionary.AddOrUpdate(job.TaskName, job, (key, job) => job);
        }

        private void CancelJob(Job job)
        {
            job.CancelationTokenSource.Cancel();
            job.IsRunning = false;
           
        }

        public bool RunNewJob(string name)
        {
            Job job = null;
            lock (locker)
            {
                if (!_jobDictionary.ContainsKey(name))
                    throw new Exception($"Job {name} does not exist!");

                job =  _jobDictionary[name];
                if(job.IsRunning)
                    throw new Exception($"Job {name} is currently running please wait!");

               

            }

            
            StartJobs(new List<Job> { job });

            if (job != null)
                return true;


            return false;


        }

       

     
        private System.Timers.Timer timer;

        private void LoadConfigFile()
        {
            if (Directory.Exists(Environment.CurrentDirectory))
            {
                string configPath = Path.Combine(Environment.CurrentDirectory, "config.json");
                if (File.Exists(configPath))
                {
                    string text = File.ReadAllText(configPath);
                    if (string.IsNullOrEmpty(text))
                        Console.WriteLine("Empty Config!!!");

                    List<Job> configs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Job>>(text);

                   
                    

                    foreach (Job config in configs)
                    {
                          try
                        {
                            LoadJob(config);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Couldnt add job {config.TaskName} during load! " + ex.Message);
                        }                                                
                    }             
                }
                else
                    Console.WriteLine("No config file found!!!");
            }
            else
                Console.WriteLine("No config file found!!!");
        }

        private void LoadJob(Job job)
        {

            if(job.TaskScheduled && job.TaskScheduledTme != null)
            {
                bool isJobPast = job.TaskScheduledTme <= DateTime.Now; // job was scheuled for some time in past
                if (isJobPast)
                    if(job.TaskScheduledTme.Value.TimeOfDay < DateTime.Now.TimeOfDay) 
                        if(job.TaskInterval <= 0) // if we missed the tick today and there in not interval do tommorow
                        {
                            job.NextExecutionTime = DateTime.Today.AddDays(1).Add(job.TaskScheduledTme.Value.TimeOfDay);
                        }
                        else // otherwise lets start it x seconds after now
                        {
                            job.NextExecutionTime = DateTime.Now.AddMinutes(job.TaskInterval);
                        }                        
                    else
                        job.NextExecutionTime = DateTime.Today.Add(job.TaskScheduledTme.Value.TimeOfDay);
                else // otherwise run today
                    job.NextExecutionTime = job.TaskScheduledTme.Value;
            }
            else if (job.TaskInterval > 0) // if the hob has an interval but no time we will schedule it for x minutes after program start
            {
                job.NextExecutionTime = DateTime.Now.AddMinutes(job.TaskInterval);
            }
            lock (locker)
            {
                if (_jobDictionary.ContainsKey(job.TaskName))
                {
                    throw new ArgumentOutOfRangeException($"There is already a job {job.TaskName}");
                }
                else
                    if (!_jobDictionary.TryAdd(job.TaskName, job))
                        throw new Exception($"Error adding {job.TaskName}");
            }

           
        }

        public List<string> GetAllJobNames()
        {
            return _jobDictionary.Keys.ToList();
        }

       


    }
}
