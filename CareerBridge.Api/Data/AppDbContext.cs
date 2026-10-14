using CareerBridge.Shared;
using Microsoft.EntityFrameworkCore;

namespace CareerBridge.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TransitionPath> TransitionPaths => Set<TransitionPath>();
    public DbSet<UserProfile> Users => Set<UserProfile>();
    public DbSet<PlaybookEntry> Playbooks => Set<PlaybookEntry>();
    public DbSet<Cohort> Cohorts => Set<Cohort>();
    public DbSet<CohortMembership> CohortMemberships => Set<CohortMembership>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<TimelineItem> TimelineItems => Set<TimelineItem>();
    public DbSet<RunwayCheckIn> RunwayCheckIns => Set<RunwayCheckIn>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Endorsement> Endorsements => Set<Endorsement>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>()
            .HasOne(u => u.TransitionPath)
            .WithMany()
            .HasForeignKey(u => u.TransitionPathId);

        modelBuilder.Entity<PlaybookEntry>()
            .HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorUserId);

        modelBuilder.Entity<PlaybookEntry>()
            .HasOne(p => p.TransitionPath)
            .WithMany()
            .HasForeignKey(p => p.TransitionPathId);
    }
}
