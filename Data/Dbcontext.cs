using Microsoft.EntityFrameworkCore;
using LottoApi.Models;

namespace LottoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Lottery> Lottery { get; set; }
       

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
            modelBuilder.Entity<Lottery>(entity =>
                {

                    entity.ToTable("Lottery");
                    entity.HasKey(e => e.lid);
                    entity.Property(e => e.lid).HasColumnName("lid");
                    entity.Property(e => e.uid).HasColumnName("uid");
                    entity.Property(e => e.price).HasColumnName("price");
                    entity.Property(e => e.number).HasColumnName("number");
                    entity.Property(e => e.start_date).HasColumnName("start_date");
                    entity.Property(e => e.end_date).HasColumnName("end_date");
                    entity.Property(e => e.status).HasColumnName("status");
                });
        }
    }
}
