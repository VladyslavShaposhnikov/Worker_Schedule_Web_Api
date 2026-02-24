using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.DTOs.Authentication;
using Worker_Schedule_Web_Api.Models.Domain;
using Worker_Schedule_Web_Api.Models.Identity;

namespace Worker_Schedule_Web_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController(UserManager<AppUser> userManager, AppDbContext appDbContext, IConfiguration configuration) : ControllerBase
    {
        [HttpPost]
        [Route("register-worker")]
        public async Task<IActionResult> RegisterWorker([FromBody] RegisterUserDto registerUserDto)
        {
            return await Register(registerUserDto, AppRoles.Worker);
        }

        [HttpPost]
        [Route("register-manager")]
        public async Task<IActionResult> RegisterManager([FromBody] RegisterUserDto registerUserDto)
        {
            return await Register(registerUserDto, AppRoles.Manager);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(loginUserDto.Email);
                if (user != null)
                {
                    if (await userManager.CheckPasswordAsync(user, loginUserDto.Password))
                    {
                        var token = await GenerateToken(user);
                        return Ok(token);
                    }
                }
                ModelState.AddModelError("", "Invalid username or password");
            }
            return BadRequest(ModelState);
        }

        public async Task<IActionResult> Register(RegisterUserDto registerUserDto, string role)
        {
            if (ModelState.IsValid)
            {
                var existedUser = await userManager.FindByEmailAsync(registerUserDto.Email);
                if (existedUser != null)
                {
                    ModelState.AddModelError("", "Email address already taken");
                    return BadRequest(ModelState);
                }
                var user = new AppUser
                {
                    UserName = registerUserDto.Email,
                    Email = registerUserDto.Email,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                using var transaction = await appDbContext.Database.BeginTransactionAsync();
                try
                {
                    var result = await userManager.CreateAsync(user, registerUserDto.Password);

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return BadRequest(ModelState);
                    }

                    var roleResult = await userManager.AddToRoleAsync(user, role);

                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return BadRequest(ModelState);
                    }

                    var worker = new Worker
                    {
                        WorkerInternalNumber = registerUserDto.WorkerInternalNumber,
                        FirstName = registerUserDto.FirstName,
                        LastName = registerUserDto.LastName,
                        EmploymentPercentage = registerUserDto.EmploymentPercentage,
                        StoreId = registerUserDto.StoreId,
                        PositionId = registerUserDto.PositionId,
                        AppUser = user
                    };

                    appDbContext.Workers.Add(worker);
                    await appDbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                var token = await GenerateToken(user);

                return Ok(token);
            }
            return BadRequest(ModelState);
        }

        private async Task<string?> GenerateToken(AppUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var secret = configuration["JwtConfig:Secret"];
            var issuer = configuration["JwtConfig:ValidIssuer"];
            var audience = configuration["JwtConfig:ValidAudiences"];
            if (secret is null || issuer is null || audience is null)
            {
                throw new ApplicationException("Jwt is not set in the configuration");
            }
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.
            GetBytes(secret));
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var securityToken = tokenHandler.
            CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            return token;
        }
    }
}
