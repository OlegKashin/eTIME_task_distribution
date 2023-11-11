namespace TaskDistribution.BLL.Models
{
    internal class Executor
    {
        public Executor(long id, long level, int workTime)
        {
            Id = id;
            Level = level;
            WorkTime = workTime;
        }

        public long Id { get; init; }

        public long Level { get; init; }

        public int WorkTime { get; private set; }

        public void SetWorkTime(int time) 
        {
            if (WorkTime - time < 0)
                return;

            WorkTime -= time;
        }
    }
}
