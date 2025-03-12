using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NodaTime;
using ProjectManager.Api.Services;
using Serilog;
using Skarabeus_Api.BackgroundServices;
using Skarabeus_Api.Settings;
using Skarabeus_Api.Utils;
using Skarabeus_Api.Utils.EmaillTemplates;
using Skarabeus_Data;
using Skarabeus_Data.Entities;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace Skarabeus_Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration().
                ReadFrom.Configuration(builder.Configuration).
                CreateLogger().ForContext<Program>();

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
                options.UseNpgsql(connectionString, builder =>
                {
                    builder.UseNodaTime();
                });
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
            });

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount = true
                )
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();


            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Events.OnRedirectToLogin = x =>
                    {
                        x.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToAccessDenied = x =>
                    {
                        x.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    };
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("UserManager", policy => policy.RequireClaim(ClaimTypes.Role, ["UserManager", "Admin"]));

            builder.Services.AddSingleton<IClock>(SystemClock.Instance);
            builder.Services.AddScoped<EmailSenderService>();
            builder.Services.AddScoped<EmailHelper>();
            builder.Services.AddHostedService<EmailSenderBackgroundService>();


            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
            builder.Services.Configure<EnvironmentSettings>(builder.Configuration.GetSection("EnvironmentSettings"));


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddTransient<SeedData>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                // Call the SeedData.Initialize method
                await SeedData.Initialize(userManager);
            }


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging();

            //app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();

            
        }
    }
}
