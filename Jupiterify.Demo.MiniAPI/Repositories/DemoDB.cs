using Microsoft.EntityFrameworkCore;

namespace Jupiterify.Demo.MiniAPI.Repositories;

class DemoDB : DbContext
{
    public DemoDB(DbContextOptions options) : base(options) { }
    public DbSet<MemberProfileEntity> MemberProfiles { get; set; } = null!;
}
