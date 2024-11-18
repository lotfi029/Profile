
using Microsoft.AspNetCore.Identity;
using Profile.Server.Models;
using Profile.Server.Settings;

namespace Profile.Server.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<User>(options)
{
    // DbSets

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .ToTable("Users");
        modelBuilder.Entity<IdentityRole>()
            .ToTable("Roles");

        //modelBuilder.Entity<IdentityRole>().HasData(new List<IdentityRole> 
        //{ 
        //    new IdentityRole { Id = Guid.NewGuid().ToString(), Name = RoleNames.Admin, NormalizedName = RoleNames.Admin.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
        //    new IdentityRole { Id = Guid.NewGuid().ToString(), Name = RoleNames.User, NormalizedName = RoleNames.User.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() }
        //});
        modelBuilder.Entity<IdentityUserLogin<string>>()
            .ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserRole<string>>()
            .ToTable("UserRoles");
        modelBuilder.Entity<IdentityRoleClaim<string>>()
            .ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserClaim<string>>()
            .ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserToken<string>>()
            .ToTable("UserTokens");


    }
         
    
}
