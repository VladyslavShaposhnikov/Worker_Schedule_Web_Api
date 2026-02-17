using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
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
                        foreach (var error in result.Errors)
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

                var token = GenerateToken(user);

                return Ok(token);
            }
            return BadRequest(ModelState);
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
                        var token = GenerateToken(user);
                        return Ok(token);
                    }
                }
                ModelState.AddModelError("", "Invalid username or password");
            }
            return BadRequest(ModelState);
        }

        private string? GenerateToken(AppUser user)
        {
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
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
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
