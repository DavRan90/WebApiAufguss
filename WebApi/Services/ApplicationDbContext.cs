using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Models.Survey;

namespace WebApi.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.AufgussFriend)
                .WithMany()
                .HasForeignKey(u => u.AufgussFriendId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .Property(u => u.AufgussFriendName)
                .HasColumnName("AufgussFriendName");

            builder.Entity<SiteSettings>().HasData(new SiteSettings
            {
                Id = 1,
                MaxBookingsAufguss = 2,
                AufgussHiddenDaysInAdvance = 7,
                MaxBookingsVattenfys = 5,
                VattenfysHiddenDaysInAdvance = 7,
                MaxNews = 5
            });
        }

        public required DbSet<Event> Events { get; set; }
        public required DbSet<EventTemplate> EventTemplates { get; set; }
        public required DbSet<Entry> Entries { get; set; }
        public required DbSet<Booking> Bookings { get; set; }
        public required DbSet<Faq> Faqs { get; set; }
        public required DbSet<Survey> Surveys { get; set; }
        public required DbSet<Invite> Invites { get; set; }
        public required DbSet<SiteSettings> SiteSettings { get; set; }
        public required DbSet<SurveyQuestion> SurveyQuestions { get; set; }
        public required DbSet<SurveyAnswer> SurveyAnswers { get; set; }
        public required DbSet<SurveyAnswerDetail> SurveyAnswerDetails { get; set; }
        public DbSet<BookingAudit> BookingAudits { get; set; }
        public DbSet<EventAudit> EventAudits { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }
}
