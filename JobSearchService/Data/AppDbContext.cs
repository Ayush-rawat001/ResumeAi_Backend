using Microsoft.EntityFrameworkCore;
using JobSearchService.Models;

namespace JobSearchService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<JobMatch> JobMatches { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<JobMatch>().ToTable("JobMatches");
        }
    }
}
