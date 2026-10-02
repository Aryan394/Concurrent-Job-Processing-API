using ConcurrentJobProcessor.Data;
using ConcurrentJobProcessor.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcurrentJobProcessor.Services;

public class JobStore(JobDbContext dbContext) : IJobStore
{
    private readonly JobDbContext _dbContext = dbContext;

    public async Task AddAsync(Job job)
    {
        _dbContext.Jobs.Add(job);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Job?> GetAsync(Guid id)
    {
        return await _dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Job job)
    {
        _dbContext.Jobs.Update(job);

        await _dbContext.SaveChangesAsync();
    }
}