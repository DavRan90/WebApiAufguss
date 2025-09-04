using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }
        public required DbSet<Event> Events { get; set; }
        public required DbSet<Entry> Entries { get; set; }
    }
}
