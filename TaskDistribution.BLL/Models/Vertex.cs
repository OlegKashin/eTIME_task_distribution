namespace TaskDistribution.BLL.Models
{
    internal struct Vertex
    {
        private Task _task;
        private Executor _executor;

        public Vertex(Task task, Executor executor)
        {
            _task = task;
            _executor = executor;
        }

        public int Weight { 
            get {
                
                if (_executor.WorkTime - _task.Time < 0)
                    return 0;

                return (int)(_task.Time * _task.Priority * _executor.Level * (_task.Priority > _executor.Level ? 0 : 1));
            }
        }
    }
}
