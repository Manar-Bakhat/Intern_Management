using Intern_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Intern_Management.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        public virtual DbSet<User> User { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<AcademicQualification> AcademicQualifications { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Certificate> Certificates { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.PermissionCode, rp.RoleCode });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionCode);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleCode);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Candidate>()
                .HasBaseType<User>();

            modelBuilder.Entity<Supervisor>()
                .HasBaseType<User>();

            modelBuilder.Entity<Candidate>()
                .HasMany(c => c.AcademicQualifications)
                .WithOne(aq => aq.Candidate)
                .HasForeignKey(aq => aq.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Candidate>()
                .HasMany(c => c.Experiences)
                .WithOne(e => e.Candidate)
                .HasForeignKey(e => e.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Candidate>()
                .HasMany(c => c.Skills)
                .WithOne(s => s.Candidate)
                .HasForeignKey(s => s.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.Request)
                .WithOne(r => r.Candidate)
                .HasForeignKey<Request>(r => r.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.Interview)
                .WithOne(i => i.Candidate)
                .HasForeignKey<Interview>(i => i.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Supervisor>()
                .HasMany(s => s.AssignedCandidates)
                .WithOne(c => c.Supervisor)
                .HasForeignKey(c => c.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificate>() // Configuration de l'entité Certificate
                .HasKey(c => c.Id);

            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Request)
                .WithMany(r => r.Certificates)
                .HasForeignKey(c => c.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
