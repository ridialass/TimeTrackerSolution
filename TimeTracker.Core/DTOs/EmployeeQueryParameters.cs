using TimeTracker.Core.Enums;

namespace TimeTracker.Core.DTOs
{
    public class EmployeeQueryParameters
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Filtrage de base (exemples)
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Town { get; set; }
        public string? Country { get; set; }
        public UserRole? Role { get; set; }
    }
}