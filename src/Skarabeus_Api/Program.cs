
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NodaTime;
using ProjectManager.Api.Services;
using Skarabeus_Api.BackgroundServices;
using Skarabeus_Api.Settings;
using Skarabeus_Data;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
                options.UseNpgsql(connectionString, builder =>
                {
                    builder.UseNodaTime();
                });
            });


            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount = true
                )
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();



            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            builder.Services.AddSingleton<IClock>(SystemClock.Instance);
            builder.Services.AddScoped<EmailSenderService>();
            builder.Services.AddHostedService<EmailSenderBackgroundService>();


            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
