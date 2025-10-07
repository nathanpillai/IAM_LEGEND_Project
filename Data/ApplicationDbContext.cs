using IAMLegend.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace IAMLegend.Data
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

        public DbSet<Domain> Domains => Set<Domain>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<LocalSystem> LocalSystems => Set<LocalSystem>();
        public DbSet<PermissionLevel> PermissionLevels => Set<PermissionLevel>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<UserSystemAccess> UserSystemAccesses => Set<UserSystemAccess>();
        public DbSet<UserSystemBranch> UserSystemBranches => Set<UserSystemBranch>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Set default schema for all tables
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<Branch>().HasKey(b => b.branchcode);
            modelBuilder.Entity<Branch>().Property(b => b.branchcode).HasMaxLength(20);

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("userprofile","public"); // <-- exact table name in PostgreSQL

                entity.HasIndex(u => new { u.domain, u.username })
                      .IsUnique(false); // duplicates handled in service

                entity.Property(u => u.email).HasMaxLength(250);
                entity.Property(u => u.username).HasMaxLength(150);
            });
        }


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<Branch>().HasKey(b => b.BranchCode);
        //    modelBuilder.Entity<Branch>().Property(b => b.BranchCode).HasMaxLength(20);

        //    modelBuilder.Entity<UserProfile>()
        //        .HasIndex(u => new { u.Domain, u.UserName })
        //        .IsUnique(false); // keep duplicates prevented by service

        //    // Default string length constraints
        //    modelBuilder.Entity<UserProfile>().Property(u => u.Email).HasMaxLength(250);
        //    modelBuilder.Entity<UserProfile>().Property(u => u.UserName).HasMaxLength(150);
        //}
    }

}
