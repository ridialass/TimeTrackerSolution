using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeTracker.Core.DTOs;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TimeTracker.AdminUI.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DetailsTimeEntryModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DetailsTimeEntryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public TimeEntryDto? TimeEntry { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwt = Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(jwt))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
            }

            var response = await client.GetAsync($"api/timeentries/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TimeEntry = null;
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            TimeEntry = JsonSerializer.Deserialize<TimeEntryDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return Page();
        }
    }
}