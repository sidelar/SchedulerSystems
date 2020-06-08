Simple Command Line Tool to run arbitrary tasks buil in .NET Core

Main Project is SchedulerSystem, which prvies a simple command line interface to run tasks
1. start {task name} - start a task not already running
2. stop {task name} - stop a task that is running

Tasks are defined in the Config.Json file
    "task_name": string,
		"task_location": path to file to execute, either full path or a file located in the executing directory,
		"task_interval": in minutes,
		"task_scheduled_time": time to start task,
		"task_scheduled": bool  

Project SchedulerSytemTests contains some simple tests processes
Project Basic Awaiter is a simple command line project that waits 1 minute and then exits.
