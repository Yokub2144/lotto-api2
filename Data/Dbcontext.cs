using Microsoft.EntityFrameworkCore;
using LottoApi.Models;

namespace LottoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User"); // ใช้ตารางเดิม
                entity.HasKey(e => e.uid);

                entity.Property(e => e.uid).HasColumnName("uid");
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.password).HasColumnName("password");
                entity.Property(e => e.fullname).HasColumnName("fullname");
                entity.Property(e => e.birthday).HasColumnName("birthday");
                entity.Property(e => e.phone).HasColumnName("phone");
                entity.Property(e => e.role).HasColumnName("role");
            });
        }
    }
}
