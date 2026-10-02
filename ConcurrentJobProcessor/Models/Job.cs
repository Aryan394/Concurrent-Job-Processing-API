using System.ComponentModel.DataAnnotations;

namespace ConcurrentJobProcessor.Models
{
    public class Job
    {
        [Required]
        public Guid Id {get; set;}
        public string? Name { get; set; }
        public string? Payload { get; set; } 
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}