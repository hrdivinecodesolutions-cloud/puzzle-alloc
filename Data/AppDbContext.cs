using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using puzzle_alloc.Models.Entities;

namespace puzzle_alloc.Data
{

    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RuleSet> RuleSets => Set<RuleSet>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Container> Containers => Set<Container>();
        public DbSet<Allocation> Allocations => Set<Allocation>();


        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            b.Entity<Container>().HasIndex(c => c.Index).IsUnique();

            b.Entity<Container>()
                .Property(c => c.CurrentLoad)
                .HasPrecision(18, 4); 

            b.Entity<Item>()
                .Property(i => i.Weight)
                .HasPrecision(18, 4); 

            b.Entity<Allocation>()
                .HasOne(a => a.Item).WithMany().HasForeignKey(a => a.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Allocation>()
                .HasOne(a => a.Container).WithMany().HasForeignKey(a => a.ContainerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    }
