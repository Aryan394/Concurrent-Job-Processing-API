using ConcurrentJobProcessor.Models;

namespace ConcurrentJobProcessor.Services
{
    public interface IJobStore
    {
        void Add(Job job);
        Job? Get(Guid id);
        void Update(Job job);
    }
}