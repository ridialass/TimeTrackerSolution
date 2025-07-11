

namespace TimeTracker.Core.DTOs
{
    public class ErrorResponseDto
    {
        public string Code { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string? Target { get; set; }
        public IDictionary<string, string[]>? Errors { get; set; }
    }
}