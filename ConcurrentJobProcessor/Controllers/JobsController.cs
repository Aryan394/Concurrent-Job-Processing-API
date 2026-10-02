using ConcurrentJobProcessor.Models;
using ConcurrentJobProcessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConcurrentJobProcessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController(IJobQueue jobQueue) : ControllerBase
    {
        private readonly IJobQueue _jobQueue = jobQueue;

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] JobRequest jobRequest)
        {
        var job = new Job
            {
                Id = Guid.NewGuid(),
                Name = jobRequest.Name,
                Payload = jobRequest.Payload,
                Status = "Queued",
                CreatedAt = DateTime.UtcNow
            };
        
        await _jobQueue.EnqueueJobAsync(job);

        return Ok(job);
        }
    }
}