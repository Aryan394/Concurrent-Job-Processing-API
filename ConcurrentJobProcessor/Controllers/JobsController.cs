using ConcurrentJobProcessor.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConcurrentJobProcessor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController: ControllerBase
    {
        [HttpPost]
        public IActionResult CreateJob([FromBody] Models.JobRequest jobRequest)
        {
        var job = new Job
            {
                Id = Guid.NewGuid(),
                Name = jobRequest.Name,
                Payload = jobRequest.Payload,
                Status = "Queued",
                CreatedAt = DateTime.UtcNow
            };

        return Ok(job);
        }
    }
}