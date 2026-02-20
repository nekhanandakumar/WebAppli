using Microsoft.EntityFrameworkCore;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");

                // PRIMARY KEY
                entity.HasKey(e => e.EmployeeID);

                entity.Property(e => e.EmployeeID)
                      .ValueGeneratedOnAdd();

                // BASIC INFO
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Designation)
                      .HasMaxLength(100);

                entity.Property(e => e.Address)
                      .HasMaxLength(200);

                entity.Property(e => e.Department)
                      .HasMaxLength(100);

                entity.Property(e => e.JoiningDate)
                      .IsRequired(false);

                entity.Property(e => e.Skillset)
                      .HasMaxLength(200);

                // IMAGE
                entity.Property(e => e.ProfileImage)
                      .HasColumnType("varbinary(max)");

                // LOGIN
                entity.Property(e => e.Username)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasIndex(e => e.Username)
                      .IsUnique();   // prevent duplicate username

                entity.Property(e => e.Password)
                      .IsRequired();

                // STATUS + ROLE
                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasDefaultValue("Active");

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(50)
                      .HasDefaultValue("User");

                // AUDIT
                entity.Property(e => e.CreatedBy)
                      .HasMaxLength(50);

                entity.Property(e => e.ModifiedBy)
                      .HasMaxLength(50);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.ModifiedAt);
            });
        }
    }
}
