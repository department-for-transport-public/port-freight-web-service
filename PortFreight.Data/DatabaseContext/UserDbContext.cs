using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortFreight.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PortFreight.Data
{
    public class UserDbContext : IdentityDbContext<PortFreightUser>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<PreApprovedUser> PreApprovedUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PortFreightUser>(b =>
            {
                b.ToTable("PortFreightUsers");
            });

            modelBuilder.Entity<PortFreightUser>(b =>
            {
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();
            });
        }
    }


    public class PortFreightUser : IdentityUser
    {
        [Required]
        public string SenderId { get; set; }

        [Required]
        public override string Email { get; set; }

        [Required]
        public override string UserName { get; set; }

        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; }

        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }

    }   
}