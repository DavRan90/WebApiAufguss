using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Mail;
using System.Security.Claims;
using System.Text;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;
using WebApi.Services.Email;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("Postgres")));

            var app = builder.Build();

            app.MapGet("/health", () => "API is alive");

            app.Run();

        }
    }

}
