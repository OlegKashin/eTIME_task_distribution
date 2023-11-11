namespace TaskDistribution.BLL.Models
{
    internal struct Task
    {
        public long Id { get; init; }
        public int Priority { get; init; }
        public int Time { get; init; }
    }
}
