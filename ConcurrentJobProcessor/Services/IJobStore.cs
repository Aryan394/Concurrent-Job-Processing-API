using ConcurrentJobProcessor.Models;

namespace ConcurrentJobProcessor.Services
{
    public interface IJobStore
    {
        Task AddAsync(Job job);
        Task<Job?> GetAsync(Guid id);
        Task UpdateAsync(Job job);
    }
}