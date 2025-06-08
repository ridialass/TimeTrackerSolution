using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Enums;

namespace TimeTracker.AdminUI.Pages.Account
{
    [Authorize] // s�assure que seul un utilisateur connect� peut atteindre cette page
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public string Username { get; set; } = "";
        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string Password { get; set; } = "";

        [BindProperty]
        public string Role { get; set; } = "Employee";

        [BindProperty]
        public string FirstName { get; set; } = "";

        [BindProperty]
        public string LastName { get; set; } = "";

        [BindProperty]
        public string Town { get; set; } = "";

        [BindProperty]
        public string Country { get; set; } = "";

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public RegisterModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void OnGet()
        {
            // On peut v�rifier ici que l'utilisateur a bien le r�le "Admin"
            if (!User.IsInRole("Admin"))
            {
                // Rediriger ou afficher une erreur
                Response.Redirect("/Account/AccessDenied");
            }
        }

        //public async Task<IActionResult> OnPostAsync()
        //{
        //    // 1) V�rifier les champs obligatoires
        //    if (string.IsNullOrWhiteSpace(Username) ||
        //        string.IsNullOrWhiteSpace(FirstName) ||
        //        string.IsNullOrWhiteSpace(LastName) ||
        //        string.IsNullOrWhiteSpace(Password))
        //    {
        //        ErrorMessage = "Tous les champs sont obligatoires.";
        //        return Page();
        //    }

        //    // 2) Construire l'EmployeeDto que l'API attend
        //    var newEmployee = new EmployeeDto
        //    {
        //        Username = Username.Trim(),
        //        Email = Email.Trim(),
        //        FirstName = FirstName.Trim(),
        //        LastName = LastName.Trim(),
        //        Town = Town.Trim(),
        //        Country = Country.Trim(),
        //        Role = Enum.TryParse<UserRole>(Role, out var r) ? r : UserRole.Employee
        //    };

        //    // 3) S�rialiser le DTO
        //    var json = JsonSerializer.Serialize(newEmployee);
        //    var content = new StringContent(json, Encoding.UTF8, "application/json");

        //    // 4) Pr�parer HttpClient et ajouter l'en-t�te Authorization avec le token
        //    var client = _httpClientFactory.CreateClient("TimeTrackerAPI");

        //    // 4a) R�cup�rer le JWT stock� dans le cookie s�curis�
        //    var jwtToken = Request.Cookies["jwt_token"];
        //    if (string.IsNullOrWhiteSpace(jwtToken))
        //    {
        //        ErrorMessage = "Vous devez �tre connect� avec un compte Admin pour cr�er un utilisateur.";
        //        return Page();
        //    }

        //    // 4b) Ajouter l�en-t�te Bearer
        //    client.DefaultRequestHeaders.Authorization =
        //    new AuthenticationHeaderValue("Bearer", jwtToken);

        //    // 5) Concat�ner l�URL et passer le param�tre password en query string
        //    var endpoint = $"api/auth/register?password={Uri.EscapeDataString(Password)}";

        //    // 6) Envoyer la requ�te POST vers l�API
        //    var response = await client.PostAsync(endpoint, content);

        //    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        //    {
        //        // 409 Conflict = Username d�j� existant
        //        ErrorMessage = "Nom d�utilisateur d�j� existant.";
        //        return Page();
        //    }
        //    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //    {
        //        // 401 Unauthorized = le token n�a pas la permission ou n�est pas valide
        //        ErrorMessage = "Vous n�avez pas les droits requis pour cr�er un utilisateur.";
        //        return Page();
        //    }
        //    else if (!response.IsSuccessStatusCode)
        //    {
        //        // Autre code d�erreur
        //        ErrorMessage = $"Erreur c�t� serveur : {(int)response.StatusCode} {response.ReasonPhrase}";
        //        return Page();
        //    }

        //    // Test API : lire la r�ponse pour v�rifier que l�utilisateur a �t� cr��
        //    // --- Appel � l�API et diagnostic du 400 ---
        //    var body = await response.Content.ReadAsStringAsync();

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        ErrorMessage = $"Erreur {(int)response.StatusCode} : {body}";
        //        return Page();
        //    }

        //    // 7) Si tout s�est bien pass� :
        //    SuccessMessage = "Inscription r�ussie ! L�utilisateur a �t� cr��.";
        //    // On peut effacer le formulaire
        //    Username = "";
        //    FirstName = "";
        //    LastName = "";
        //    Password = "";
        //    Role = "Employee";

        //    return Page();
        //}

        public async Task<IActionResult> OnPostAsync()
        {
            // 1) Validation client simple
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Tous les champs sont obligatoires.";
                return Page();
            }

            // 2) Cr�er le DTO que l'API attend
            var registerDto = new RegisterRequestDto
            {
                Username = Username.Trim(),
                Email = Email.Trim(),
                Password = Password,  // ici !
                Role = Enum.Parse<UserRole>(Role),
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Town = Town.Trim(),
                Country = Country.Trim()
            };

            // 3) S�rialiser et pr�parer le content
            var json = JsonSerializer.Serialize(registerDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 4) Pr�parer HttpClient avec le Bearer token
            var client = _httpClientFactory.CreateClient("TimeTrackerAPI");
            var jwtToken = Request.Cookies["jwt_token"];
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                ErrorMessage = "Vous devez �tre connect� en tant qu�Admin.";
                return Page();
            }
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", jwtToken);

            // 5) POST sans query-string
            var response = await client.PostAsync("api/auth/register", content);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                ErrorMessage = "Nom d�utilisateur d�j� existant.";
                return Page();
            }
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Erreur {(int)response.StatusCode} : {body}";
                return Page();
            }

            // 6) Succ�s
            SuccessMessage = "Inscription r�ussie !";
            // r�initialiser le form�
            Username = Email = FirstName = LastName = Password = Town = Country = "";
            Role = UserRole.Employee.ToString();
            return Page();
        }

    }
}
