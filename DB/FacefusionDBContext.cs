using Microsoft.EntityFrameworkCore;

namespace FacefusionBE.DB
{
    public class FacefusionDBContext : DbContext
    {
        public FacefusionDBContext(DbContextOptions<FacefusionDBContext> options) : base(options)
        { }
        public DbSet<DBUser> Users { get; set; }

        public DbSet<DBUserSession> UserSessions { get; set; }
    }
}
