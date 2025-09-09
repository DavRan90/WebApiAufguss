using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public required DbSet<Event> Events { get; set; }
        public required DbSet<Entry> Entries { get; set; }
        public required DbSet<Booking> Bookings { get; set; }
    }
}
