using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace TimeTracker.AdminUI.Pages;
public class UserPageModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public UserPageModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public string FirstName { get; set; } = "";
    public string? LastName { get; set; } = "";
    public string? UserImg { get; set; } = null;
    public bool IsAdmin { get; set; }


    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
        var response = await client.GetAsync("api/employees/me");
        if (!response.IsSuccessStatusCode) return;
        var employee = JsonSerializer.Deserialize<EmployeeDto>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (employee != null)
        {
            FirstName = employee.FirstName ?? "";
            LastName = employee.LastName;
            UserImg = employee.ProfilePictureUrl;
            IsAdmin = employee.IsAdmin;
        }
    }
    

    private sealed class EmployeeDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsAdmin { get; set; }
    }
}

