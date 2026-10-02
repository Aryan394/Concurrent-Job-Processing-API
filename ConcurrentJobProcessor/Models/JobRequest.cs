namespace ConcurrentJobProcessor.Models
{
    public class JobRequest
    {
        public string? Name { get; set; }

        public string? Payload { get; set; }
    }
}