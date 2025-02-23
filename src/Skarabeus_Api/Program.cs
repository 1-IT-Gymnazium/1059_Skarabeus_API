using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using Skarabeus_Data;
using Skarabeus_Data.Entities;
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
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            builder.Services.AddAuthorization(options =>
             {
                 options.AddPolicy("UserManager", policy => policy.RequireClaim(ClaimTypes.Role, ["UserManager", "Admin"]));
             });
            /*
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));

            var jwtSettings = builder.Configuration.GetRequiredSection(nameof(JwtSettings)).Get<JwtSettings>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience
                };
            });
            */
            builder.Services.AddSingleton<IClock>(SystemClock.Instance);
            builder.Services.AddScoped<EmailSenderService>();
            builder.Services.AddHostedService<EmailSenderBackgroundService>();


            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                /*
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT API", Version = "v1" });
                
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token without the 'Bearer' prefix.\n\nExample: abc123xyz"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                */
            });

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
