using System.Threading.Channels;
using ConcurrentJobProcessor.Models;

namespace ConcurrentJobProcessor.Services
{
    public class JobQueue : IJobQueue
    {
        private readonly Channel<Job> _queue;
        public JobQueue()
        {
            _queue = Channel.CreateUnbounded<Job>();
        }
        public async ValueTask EnqueueJobAsync(Job job)
        {
            await _queue.Writer.WriteAsync(job);
        }
        public async ValueTask<Job> DequeueJobAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}