using Microsoft.EntityFrameworkCore;
using LottoApi.Models;

namespace LottoApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Lottery> Lottery { get; set; }
        public DbSet<Reward> Reward { get; set; }

        public DbSet<Wallet> Wallet { get; set; }

        public DbSet<Order> Orders { get; set; }

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
            modelBuilder.Entity<Reward>(entity =>
          {
              entity.ToTable("reward");
              entity.HasKey(e => e.Rid);
              entity.Property(e => e.Rid).HasColumnName("rid");
              entity.Property(e => e.Lid).HasColumnName("lid");
              entity.Property(e => e.Rank).HasColumnName("rank");
          });
            modelBuilder.Entity<Wallet>(entity =>
           {
               entity.ToTable("wallet");
               entity.HasKey(e => e.wid);
               entity.Property(e => e.wid).HasColumnName("wid");
               entity.Property(e => e.uid).HasColumnName("uid");
               entity.Property(e => e.money).HasColumnName("money");
               entity.Property(e => e.account_id)
            .HasColumnName("account_id")
            .HasMaxLength(50)
            .IsRequired(false);
           });
            modelBuilder.Entity<Order>(entity =>
{
    entity.ToTable("Orders");        // ชื่อตารางจริงใน database
    entity.HasKey(e => e.oid);           // primary key

    entity.Property(e => e.oid)
        .HasColumnName("oid")
        .ValueGeneratedOnAdd();           // AUTO_INCREMENT

    entity.Property(e => e.uid)
        .HasColumnName("uid")
        .IsRequired();

    entity.Property(e => e.lid)
        .HasColumnName("lid")
        .IsRequired();

    entity.Property(e => e.date)
        .HasColumnName("date")
        .IsRequired();

    entity.Property(e => e.amount)
        .HasColumnName("amount")
        .HasDefaultValue(1)               // default = 1
        .IsRequired();

    entity.Property(e => e.statusbonus)
        .HasColumnName("statusbonus")
        .HasMaxLength(50)
        .HasDefaultValue("ยังไม่ขึ้นรางวัล")
        .IsRequired();
});

        }
    }
}
