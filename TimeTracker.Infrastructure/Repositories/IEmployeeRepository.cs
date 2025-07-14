using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Core.DTOs;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Repositories
{
    public interface IEmployeeRepository
    {
        Task<ApplicationUser?> FindByUsernameOrEmailAsync(string? identifier);
        Task<ApplicationUser?> FindByUsernameAsync(string username);
        Task<ApplicationUser?> FindByEmailAsync(string email);
        Task<ApplicationUser?> FindByPasswordResetTokenAsync(string token);
        Task<bool> ExistsByUsernameOrEmailAsync(string username, string? email);
        Task CreateAsync(ApplicationUser user); // Utilisé pour l'inscription, tu peux garder ou remplacer par AddAsync
        Task<bool> UpdateAsync(ApplicationUser user); //Signature modifiée pour correspondre à la méthode de mise à jour
        bool VerifyPassword(ApplicationUser user, string password);
        string HashPassword(string password);

        // Ajoute ces méthodes pour compatibilité avec EmployeeService
        Task<ApplicationUser> AddAsync(ApplicationUser user); // Pour la création (CreateEmployeeAsync)
        Task<bool> DeleteAsync(int id); // Pour la suppression
        Task<IEnumerable<ApplicationUser>> GetAllAsync(); // Pour la liste
        Task<ApplicationUser?> GetByIdAsync(int id); // Pour la récupération par id

        Task<(IEnumerable<ApplicationUser> Items, int TotalCount)> GetPagedAsync(EmployeeQueryParameters query);
    }
}