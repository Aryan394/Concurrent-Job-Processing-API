using ConcurrentJobProcessor.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcurrentJobProcessor.Data;
public class JobDbContext(DbContextOptions<JobDbContext> options): DbContext(options)
{
    public DbSet<Job> Jobs => Set<Job>();
}