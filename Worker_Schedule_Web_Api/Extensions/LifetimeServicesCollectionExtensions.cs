using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Worker_Schedule_Web_Api.Data;
using Worker_Schedule_Web_Api.Models.Identity;
using Worker_Schedule_Web_Api.Services;
using Worker_Schedule_Web_Api.Services.Interfaces;

namespace Worker_Schedule_Web_Api.Extensions
{
    public static class LifetimeServicesCollectionExtensions
    {
        public static IServiceCollection AddLifetimeServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddHttpContextAccessor();
            
            services.AddScoped<IAvailabilityService, AvailabilityService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IScheduler, SchedulerService>();
            services.AddScoped<IShiftDemandService, ShiftDemandService>();
            services.AddScoped<ISchedulingAlgorithm, SchedulingAlgorithm>();
            services.AddScoped<IScheduleMonthAlgorithm, ScheduleMonthAlgorithm>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddDbContext<AppDbContext>();
            services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var secret = configuration["JwtConfig:Secret"];
                var issuer = configuration["JwtConfig:ValidIssuer"];
                var audience = configuration["JwtConfig:ValidAudiences"];
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
            services.AddOpenApi();

            return services;
        }
    }
}
