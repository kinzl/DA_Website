using Microsoft.EntityFrameworkCore;

namespace VeloMobilDb;

public class VeloMobilContext : DbContext
{
    public VeloMobilContext(DbContextOptions<VeloMobilContext> options)
        : base(options)
    {
    }

    public VeloMobilContext()
    {
    }

    public DbSet<DetailPosition> DetailPositions { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<User> Users { get; set; }
}