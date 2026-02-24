using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using puzzle_alloc.Models.Entities;

namespace puzzle_alloc.Data
{
public static class Seed
    {
        public static async Task EnsureSeedAsync(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await ctx.Database.MigrateAsync();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        
            if (!await roleMgr.RoleExistsAsync("Admin")) await roleMgr.CreateAsync(new IdentityRole("Admin"));
            if (!await roleMgr.RoleExistsAsync("User")) await roleMgr.CreateAsync(new IdentityRole("User"));

         
            async Task<AppUser> EnsureUser(string email, string role)
            {
                var u = await userMgr.FindByEmailAsync(email);
                if (u == null)
                {
                    u = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
                    await userMgr.CreateAsync(u, "P@ssw0rd!");
                    await userMgr.AddToRoleAsync(u, role);
                }
                return u;
            }
            var admin = await EnsureUser("admin@example.com", "Admin");
            var user = await EnsureUser("user@example.com", "User");



            if (!await ctx.RuleSets.AnyAsync())
            {
                ctx.RuleSets.Add(new RuleSet
                {
                    MaxCapacityPerContainer = 100,
                    EnforceCategorySeparation = true,
                    GhostContainerCount = 2,
                    Active = true,
                    CreatedByUserId = admin.Id
                });
                await ctx.SaveChangesAsync();
            }

        
            if (!await ctx.Containers.AnyAsync())
            {
                var rules = await ctx.RuleSets.FirstAsync(r => r.Active);
                for (int i = 1; i <= 5; i++)
                {
                    ctx.Containers.Add(new Container
                    {
                        Index = i,
                        CurrentLoad = 0,
                        IsGhost = i <= rules.GhostContainerCount,
                        IsActive = false
                    });
                }
                await ctx.SaveChangesAsync();
            }
        }
    }

}
