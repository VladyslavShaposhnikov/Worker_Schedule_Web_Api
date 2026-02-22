
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.Models.Identity;
using Scalar.AspNetCore;
using System.Threading.Tasks;
using Worker_Schedule_Web_Api.Services.Interfaces;
using Worker_Schedule_Web_Api.Services;
using Worker_Schedule_Web_Api.Middleware;

namespace Worker_Schedule_Web_Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var secret = builder.Configuration["JwtConfig:Secret"];
                var issuer = builder.Configuration["JwtConfig:ValidIssuer"];
                var audience = builder.Configuration["JwtConfig:ValidAudiences"];
                if (secret is null || issuer is null || audience is null)
                {
                    throw new ApplicationException("Jwt is not set in the configuration");
                }
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new
                TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            using (var scopeService = app.Services.CreateScope())
            {
                var services = scopeService.ServiceProvider;
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                if (! await roleManager.RoleExistsAsync(AppRoles.Worker))
                {
                    await roleManager.CreateAsync(new IdentityRole(AppRoles.Worker));
                }
                if (!await roleManager.RoleExistsAsync(AppRoles.Manager))
                {
                    await roleManager.CreateAsync(new IdentityRole(AppRoles.Manager));
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseMiddleware<GlobalExceptionMiddleware>();

            // for testing purposes only!!!
            app.Use(async (context, next) =>
            {
                var config = context.RequestServices.GetRequiredService<IConfiguration>();

                var token = config["TestToken"];

                if (!string.IsNullOrEmpty(token))
                {
                    context.Request.Headers.TryAdd("Authorization", $"Bearer {token}");
                }

                await next();
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
