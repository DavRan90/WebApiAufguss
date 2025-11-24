using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebApi.Services;
using System.Text;
using WebApi.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebApi.Services.Email;
using SendGrid.Helpers.Mail;
using System.Security.Claims;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //{
            //    var connectionString = builder.Configuration.GetConnectionString("MyPC");
            //    options.UseSqlServer(connectionString);
            //});

            //        builder.Services.Configure<SendGridSettings>(
            //builder.Configuration.GetSection("SendGridSettings"));
            //        Console.WriteLine("SendGrid API key: " + builder.Configuration["SendGridSettings:ApiKey"]);

            builder.Services.AddHttpClient<ResendEmailService>();

            builder.Services.AddScoped<NotificationService>();

            builder.Services.AddScoped<FriendService>();

            builder.Services.AddScoped<IBookingManager, BookingManager>();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAuditService, AuditService>();

            //builder.Services.AddScoped<EmailService>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddSingleton<IBackgroundEmailQueue, BackgroundEmailQueue>();
            builder.Services.AddHostedService<EmailBackgroundService>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            
            // 1. Add EF Core
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyPC")));

            // 2. Add Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // 3. Configure JWT auth
            var jwtKey = builder.Configuration["Jwt:Key"];
            var jwtIssuer = builder.Configuration["Jwt:Issuer"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // only require HTTPS in prod
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = ClaimTypes.Role
    };
});

            //            builder.Services.AddAuthentication(options =>
            //            {
            //                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //            })
            //.AddJwtBearer(options =>
            //{
            //    options.RequireHttpsMetadata = true;
            //    options.SaveToken = true;
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = false,
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //        IssuerSigningKey = new SymmetricSecurityKey(
            //            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            //        // New
            //        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"

            //    };
            //});

            // When i want more requirements on password

            //builder.Services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequireDigit = true;
            //    options.Password.RequireLowercase = true;
            //    options.Password.RequireUppercase = true;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequiredLength = 6;
            //});

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(
                            "https://localhost:7003",
                            "http://localhost:7003",
                            "https://localhost:5011",
                            "http://localhost:5011",
                            "https://aufgussdemo.azurewebsites.net",
                            "https://frejabadogym.se"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });


            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowFrontend",
            //        policy =>
            //        {
            //            policy.WithOrigins("https://aufgussdemo.azurewebsites.net", "https://localhost:7003") // Add all frontend origins here
            //                  .AllowAnyHeader()
            //                  .AllowAnyMethod();
            //        });
            //});

            builder.Services.AddEndpointsApiExplorer();
            

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseExceptionHandler("/error");  // Optional, adds error endpoint
            // Order is critical!
            app.UseHttpsRedirection();

            app.UseCors("AllowFrontend"); // CORS must come before auth!

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();
            app.MapControllers();
            app.Run();
            
        }
    }

}
