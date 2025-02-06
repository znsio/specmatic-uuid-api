using Microsoft.EntityFrameworkCore;
using specmatic_uuid_api.Models.Entity;

namespace specmatic_uuid_api.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<UUID> UUIDs { get; set; }
    }
}
