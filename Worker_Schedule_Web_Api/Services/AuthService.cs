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
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Services
{
    public class AuthService(UserManager<AppUser> userManager, AppDbContext appDbContext, IConfiguration configuration) : IAuthService
    {
        public async Task<ResultAuthDto> Login(LoginUserDto loginUserDto)
        {
            var user = await userManager.FindByEmailAsync(loginUserDto.Email);
            if (user != null)
            {
                if (await userManager.CheckPasswordAsync(user, loginUserDto.Password))
                {
                    return new ResultAuthDto
                    {
                        Success = true,
                        Token = await GenerateToken(user)
                    };
                }
            }
            return new ResultAuthDto
            {
                Success = false,
                Errors = ["Invalid email or password"]
            };
        }

        public async Task<ResultAuthDto> Register(RegisterUserDto registerUserDto, string role)
        {
            var existedUser = await userManager.FindByEmailAsync(registerUserDto.Email);
            if (existedUser != null)
            {
                return new ResultAuthDto
                {
                    Success = false,
                    Errors = ["Email address already taken"]
                };
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
      
                    return new ResultAuthDto
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description)
                    };
                }

                var roleResult = await userManager.AddToRoleAsync(user, role);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new ResultAuthDto
                    {
                        Success = false,
                        Errors = roleResult.Errors.Select(e => e.Description)
                    };
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

            return new ResultAuthDto
            {
                Success = true,
                Token = token
            };
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
