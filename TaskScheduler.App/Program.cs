namespace TaskScheduler.App
{

    // TASK:
    // Change the Scheduler class such that it will ensure that SchedulerTask.Calculate() is executed regularly for each SchedulerTask
    // with an interval that is as close as possible to the ScheulderTask.Interval.
    //
    // IMPORTANT: The accuracy of actual execution intervals should degrade gracefully when a *large number of tasks* is specified.
    // Try to avoid 100% total CPU utilization (i.e. not all CPU cores should be at 100% all the time).
    //
    // you can assume:
    //  - 8 CPU cores are available
    //  - SchedulerTask.Calculate() takes up near 100% CPU utilization of a single core for the duration of the function call

    public sealed class SchedulerTask // this class should not be changed
    {
        public TimeSpan Interval { get; }
        public int TaskId { get; }

        private static int nextId = 0;

        public SchedulerTask()
        {
            this.Interval = TimeSpan.FromSeconds(Random.Shared.Next(1, 10)); // run every 1 to 10 seconds
            this.TaskId = nextId++;
        }

        public void Calculate()
        {
            var startTime = DateTime.UtcNow;
            Console.WriteLine($"{startTime.ToLongTimeString()}.{startTime.Millisecond} - ID: {TaskId} - Interval: {Interval.TotalSeconds}");
            Console.Out.Flush();

            var endTime = startTime + TimeSpan.FromMilliseconds(Random.Shared.Next(500, 1500)); // run for at least 0.5, max 1.5 seconds
            while (DateTime.UtcNow < endTime) ; // keep busy
        }

        static public IEnumerable<SchedulerTask> GetTasks()
        {
            while (true)
            {
                yield return new SchedulerTask();
            }
        }
    }

    public class Scheduler
    {
        private List<SchedulerTask> tasks;

        public Scheduler(List<SchedulerTask> tasks)
        {
            this.tasks = tasks;
            StartTasksScheduling();
        }

        private void StartTasksScheduling()
        {
            Task.Run(async () =>
            {
                while (true)
                {                
                    var tasksToBeExecuted = tasks.ToList();
                             
                    await Task.WhenAll(tasksToBeExecuted.Select(async task =>
                    {
                        await Task.Delay(task.Interval);
                        task.Calculate();
                    }));

                    await Task.Delay(1000);
                }
            });
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            // add/modify code here to test with sample data (optional)
            var tasks = SchedulerTask.GetTasks().Take(999999999).ToList();
            var scheduler = new Scheduler(tasks);
        }
    }
}
