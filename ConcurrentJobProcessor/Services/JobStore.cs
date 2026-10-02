using System.Collections.Concurrent;
using ConcurrentJobProcessor.Models;

namespace ConcurrentJobProcessor.Services
{
    public class JobStore : IJobStore
    {
        private readonly ConcurrentDictionary<Guid, Job> _jobs = new();
        public void Add(Job job)
        {
            _jobs[job.Id] = job;
        }

        public Job? Get(Guid id)
        {
            _jobs.TryGetValue(id, out var job);
            return job;
        }

        public void Update(Job job)
        {
            _jobs[job.Id] = job;
        }
    }
}