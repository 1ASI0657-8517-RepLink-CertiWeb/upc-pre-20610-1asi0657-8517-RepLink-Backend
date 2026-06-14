using CertiWeb.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;
using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using CertiWeb.API.Users.Domain.Model.Aggregates;
using CertiWeb.API.Certifications.Domain.Model.Aggregates;
using CertiWeb.API.Certifications.Infrastructure;
using CertiWeb.API.IAM.Domain.Model.Aggregates;
using CertiWeb.API.IAM.Infrastructure.Persistence.EFC.Seeders;

namespace CertiWeb.API.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User Context
        builder.Entity<User>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(d => d.name).IsRequired();
            entity.Property(d => d.email).IsRequired();
            entity.Property(d => d.password).IsRequired();
            entity.Property(d => d.plan).IsRequired();
            
            entity.Property(d => d.CreatedDate).HasColumnName("created_at");
            entity.Property(d => d.UpdatedDate).HasColumnName("updated_at");
            
            entity.ToTable("users");
        });
        
        // AdminUser Context
        builder.Entity<AdminUser>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Email).IsRequired().HasMaxLength(255);
            entity.Property(a => a.Password).IsRequired().HasMaxLength(255);
            
            entity.HasIndex(a => a.Email).IsUnique();
            
            entity.Property(a => a.CreatedDate).HasColumnName("created_at");
            entity.Property(a => a.UpdatedDate).HasColumnName("updated_at");
            
            entity.ToTable("admin_users");
        });
        
        // Reservation
        builder.Entity<CertiWeb.API.Reservation.Domain.Model.Aggregates.Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(r => r.UserId).IsRequired();
            entity.Property(r => r.ReservationName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.ReservationEmail).IsRequired().HasMaxLength(100);
            entity.Property(r => r.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(r => r.Brand).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Model).IsRequired().HasMaxLength(50);
            entity.Property(r => r.LicensePlate).IsRequired().HasMaxLength(7);
            entity.Property(r => r.InspectionDateTime).IsRequired();
            entity.Property(r => r.Price).IsRequired().HasPrecision(18, 2);
            entity.Property(r => r.Status).IsRequired().HasMaxLength(20);
            
            entity.Property(r => r.CreatedDate).HasColumnName("created_at");
            entity.Property(r => r.UpdatedDate).HasColumnName("updated_at");
            
            entity.ToTable("reservations");
        });
        
        // Brand
        builder.Entity<Brand>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
            entity.Property(b => b.IsActive).IsRequired().HasDefaultValue(true);
            
            entity.ToTable("brands");
        });
        
        // Car
        builder.Entity<Car>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
            entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Owner).IsRequired().HasMaxLength(100);
            entity.Property(c => c.OwnerEmail).IsRequired().HasMaxLength(255);
            entity.Property(c => c.Model).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.ImageUrl).HasMaxLength(500);
            entity.Property(c => c.OriginalReservationId).IsRequired();
            
            // Value Objects
            entity.Property(c => c.Year)
                .HasConversion(
                    year => year.Value,
                    value => new CertiWeb.API.Certifications.Domain.Model.ValueObjects.Year(value))
                .IsRequired();
                
            entity.Property(c => c.Price)
                .HasConversion(
                    price => price.Value,
                    value => new CertiWeb.API.Certifications.Domain.Model.ValueObjects.Price(value, "SOL"))
                .HasPrecision(18, 2)
                .IsRequired();
                
            entity.Property(c => c.LicensePlate)
                .HasConversion(
                    plate => plate.Value,
                    value => new CertiWeb.API.Certifications.Domain.Model.ValueObjects.LicensePlate(value))
                .HasMaxLength(10)
                .IsRequired();
                
            entity.Property(c => c.PdfCertification)
                .HasConversion(
                    pdf => pdf.Base64Data,
                    value => new CertiWeb.API.Certifications.Domain.Model.ValueObjects.PdfCertification(value))
                .HasColumnType("longtext")  // MySQL: longtext para PDF grande
                .IsUnicode(true)
                .IsRequired();
            
            // Foreign Key
            entity.HasOne(c => c.Brand)
                .WithMany()
                .HasForeignKey(c => c.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(c => c.LicensePlate).IsUnique();
            entity.HasIndex(c => c.OriginalReservationId).IsUnique();
            
            entity.ToTable("cars");
        });
        
        builder.Entity<Brand>().HasData(BrandSeeder.GetPredefinedBrands());
        builder.Entity<AdminUser>().HasData(AdminUserSeeder.GetAdminUser());
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Car> Cars { get; set; }
}