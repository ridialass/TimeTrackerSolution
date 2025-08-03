using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace TimeTracker.AdminUI.Pages;
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public IndexModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public string FirstName { get; set; } = "";
    public string? LastName { get; set; } = "";
    public string? UserImg { get; set; } = null;
    public bool IsAdmin { get; set; }


    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
        var jwt = Request.Cookies["jwt_token"];
        if (!string.IsNullOrEmpty(jwt))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

        var resp = await client.GetAsync("api/employees/me");
        if (resp.IsSuccessStatusCode)
        {
            var json = await resp.Content.ReadAsStringAsync();
            var me = JsonSerializer.Deserialize<EmployeeDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            FirstName = me?.FirstName ?? "";
            LastName = me?.LastName ?? "";
            UserImg = me?.ProfilePictureUrl;
            IsAdmin = me?.IsAdmin ?? false;
        }
    }
    public class EmployeeDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsAdmin { get; set; }
    }
}