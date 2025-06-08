//// TimeTracker.API/Controllers/AccountController.cs
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using TimeTracker.Core.DTOs;

//namespace TimeTracker.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AccountController : ControllerBase
//    {
//        private readonly UserManager<IdentityUser> _userManager;
//        private readonly RoleManager<IdentityRole> _roleManager;
//        private readonly IConfiguration _configuration;

//        public AccountController(
//            UserManager<IdentityUser> userManager,
//            RoleManager<IdentityRole> roleManager,
//            IConfiguration configuration)
//        {
//            _userManager = userManager;
//            _roleManager = roleManager;
//            _configuration = configuration;
//        }

//        // POST: api/Account/Register
//        [HttpPost("Register")]
//        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
//        {
//            // 1) Vérifier si l’utilisateur existe déjà (facultatif)
//            var existing = await _userManager.FindByNameAsync(model.Username);
//            if (existing != null)
//                return BadRequest("Le nom d’utilisateur existe déjà.");

//            // 2) Créer l’utilisateur
//            var user = new IdentityUser
//            {
//                UserName = model.Username,
//                Email = model.Email
//            };

//            var createResult = await _userManager.CreateAsync(user, model.Password);
//            if (!createResult.Succeeded)
//                return BadRequest(createResult.Errors);

//            // 3) Créer le rôle s’il n’existe pas déjà
//            if (!await _roleManager.RoleExistsAsync(model.Role.ToString()))
//            {
//                await _roleManager.CreateAsync(new IdentityRole(model.Role.ToString()));
//            }

//            // 4) Ajouter l’utilisateur au rôle
//            await _userManager.AddToRoleAsync(user, model.Role.ToString());

//            return Ok(new { Message = "Utilisateur créé avec succès." });
//        }

//        // POST: api/Account/Login
//        [HttpPost("Login")]
//        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
//        {
//            // 1) Rechercher l’utilisateur
//            var user = await _userManager.FindByNameAsync(model.Username);
//            if (user == null)
//                return Unauthorized("Identifiants invalides.");

//            // 2) Vérifier le mot de passe
//            if (!await _userManager.CheckPasswordAsync(user, model.Password))
//                return Unauthorized("Identifiants invalides.");

//            // 3) Récupérer les rôles pour les ajouter au token
//            var roles = await _userManager.GetRolesAsync(user);

//            // 4) Construire la liste des claims
//            var authClaims = new List<Claim>
//            {
//                new Claim(ClaimTypes.Name, user.UserName!),
//                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//            };

//            foreach (var role in roles)
//            {
//                authClaims.Add(new Claim(ClaimTypes.Role, role));
//            }

//            // 5) Générer le token JWT
//            var authSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!)
//            );

//            var token = new JwtSecurityToken(
//                issuer: _configuration["Jwt:Issuer"],
//                audience: _configuration["Jwt:Audience"],
//                expires: DateTime.UtcNow.AddHours(3),
//                claims: authClaims,
//                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
//            );

//            return Ok(new
//            {
//                token = new JwtSecurityTokenHandler().WriteToken(token),
//                expiration = token.ValidTo,
//                roles = roles
//            });
//        }

//        // GET: api/Account/Protected
//        [HttpGet("Protected")]
//        [Authorize(Policy = "RequireAdminRole")]
//        public IActionResult AdminOnlyEndpoint()
//        {
//            return Ok("Vous êtes authentifié en tant qu’Admin !");
//        }
//    }

//}
