
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using Worker_Schedule_Web_Api.Extensions;
using Worker_Schedule_Web_Api.Middleware;
using Worker_Schedule_Web_Api.Models.Identity;

namespace Worker_Schedule_Web_Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:8000") 
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Add services to the container.

            builder.Services.AddLifetimeServices(builder.Configuration);

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
                if (!await roleManager.RoleExistsAsync(AppRoles.VisualMerchandiser))
                {
                    await roleManager.CreateAsync(new IdentityRole(AppRoles.VisualMerchandiser));
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
            //app.Use(async (context, next) =>
            //{
            //    var config = context.RequestServices.GetRequiredService<IConfiguration>();

            //    var token = config["TestToken"];

            //    if (!string.IsNullOrEmpty(token))
            //    {
            //        context.Request.Headers.TryAdd("Authorization", $"Bearer {token}");
            //    }

            //    await next();
            //});

            app.UseHttpsRedirection();

            app.UseCors("AllowAngular");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
