using ConcurrentJobProcessor.Models;
using ConcurrentJobProcessor.Services;
using ConcurrentJobProcessor.Common;
using Microsoft.AspNetCore.Mvc;

namespace ConcurrentJobProcessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController(IJobQueue jobQueue, IJobStore jobStore) : ControllerBase
    {
        private readonly IJobQueue _jobQueue = jobQueue;
        private readonly IJobStore _jobStore = jobStore;

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] JobRequest jobRequest)
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                Name = jobRequest.Name,
                Payload = jobRequest.Payload,
                Status = JobStatus.Queued.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _jobStore.AddAsync(job);
            await _jobQueue.EnqueueJobAsync(job);
            return Ok(job);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(Guid id)
        {
            var job = _jobStore.GetAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }
    }
}