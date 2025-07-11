using System;

namespace TimeTracker.Core.DTOs
{
    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}