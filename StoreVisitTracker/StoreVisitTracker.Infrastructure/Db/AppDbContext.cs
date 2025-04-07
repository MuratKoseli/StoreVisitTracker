using System;
using Microsoft.EntityFrameworkCore;
using StoreVisitTracker.Domain.Entities;

namespace StoreVisitTracker.Infrastructure.Db
{
      public class AppDbContext : DbContext
      {
            public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
            { }

            public DbSet<User> Users => Set<User>();
            public DbSet<Store> Stores => Set<Store>();
            public DbSet<Visit> Visits => Set<Visit>();
            public virtual DbSet<Product> Products => Set<Product>(); // Moq kütüphanesinin ovveride edebilmesi için virtual yaptık.
            public DbSet<Photo> Photos => Set<Photo>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {

                  // Date Time özellikleri Entity'ler üzerinden C# tarafından tanımlandı. Çünkü MySQL'de datetime(6) sorun çıkartıyor.


                  
                  modelBuilder.Entity<User>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        entity.Property(e => e.Username).HasMaxLength(255).IsRequired();
                        entity.Property(e => e.Role)
                        .HasConversion<string>()
                        .HasMaxLength(50)
                        .IsRequired();
                  });

                  
                  modelBuilder.Entity<Store>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                        entity.Property(e => e.Location).HasMaxLength(255);

                  });

                  
                  modelBuilder.Entity<Visit>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();

                        entity.HasOne(v => v.User)
                        .WithMany(u => u.Visits)
                        .HasForeignKey(v => v.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(v => v.Store)
                        .WithMany(s => s.Visits)
                        .HasForeignKey(v => v.StoreId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                
                  modelBuilder.Entity<Product>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                        entity.Property(e => e.Category).HasMaxLength(100);
                  });

                  
                  modelBuilder.Entity<Photo>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Id).ValueGeneratedOnAdd();
                        entity.Property(e => e.Base64Image).HasColumnType("LONGTEXT").IsRequired();

                        entity.HasOne(p => p.Visit)
                        .WithMany(v => v.Photos)
                        .HasForeignKey(p => p.VisitId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(p => p.Product)
                        .WithMany(pr => pr.Photos)
                        .HasForeignKey(p => p.ProductId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });
            }
      }
}
