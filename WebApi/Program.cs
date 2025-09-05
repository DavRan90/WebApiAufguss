using Microsoft.EntityFrameworkCore;
using WebApi.Services;

namespace WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString);
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseStaticFiles(); // Serves wwwroot/*

            app.MapControllers();

            app.Run();
        }
    }
}
