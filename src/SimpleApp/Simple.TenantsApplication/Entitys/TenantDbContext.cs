using Microsoft.EntityFrameworkCore;
using Simple.EntityFrameworkCore;

namespace Simple.TenantsApplication.Entitys
{
    public class TenantDbContext : AppDbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenant { get; set; }
    }
}