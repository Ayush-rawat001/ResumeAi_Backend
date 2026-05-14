using Microsoft.EntityFrameworkCore;
using AIService.Entities;

namespace AIService.Data
{
    public class AiDbContext : DbContext
    {
        public AiDbContext(DbContextOptions<AiDbContext> options) : base(options)
        {
        }

        public DbSet<AiRequest> AiRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Any specific configurations can go here
        }
    }
}
