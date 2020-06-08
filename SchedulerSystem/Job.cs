using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SchedulerSystem
{
    class Job
    {
        [JsonProperty("task_name")]
        public string TaskName { get; set; }
        [JsonProperty("task_location")]
        public string TaskLocation { get; set; }
        [JsonProperty("task_interval")]
        public int TaskInterval { get; set; }
        [JsonProperty("task_scheduled_time")]
        public DateTime? TaskScheduledTme { get; set; } = null;
        [JsonProperty("task_scheduled")]
        public bool TaskScheduled{ get; set; }

        public bool IsCancelable { get; set; }


        public DateTime? NextExecutionTime { get; set; }

        public bool IsRunning { get; set; }
        public CancellationTokenSource CancelationTokenSource { get; internal set; }
    }
}
