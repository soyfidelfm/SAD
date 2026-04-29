using Microsoft.EntityFrameworkCore;
using Sad.Api.Data.Entities;
using Sad.Api.Data.Entities.Sales;
using SADWebApi.Data.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Sad.Api.Data;

public class SadDbContext : DbContext
{
    public SadDbContext(DbContextOptions<SadDbContext> options) : base(options) { }

    // catalog
    public DbSet<CatalogIdentityProvider> IdentityProviders => Set<CatalogIdentityProvider>();
    public DbSet<CatalogStore> Stores => Set<CatalogStore>();
    public DbSet<CatalogSaleStatus> SaleStatus => Set<CatalogSaleStatus>();
    public DbSet<CatalogCreditCardProduct> CreditCardProducts => Set<CatalogCreditCardProduct>();
    public DbSet<CatalogMembershipProduct> MembershipProducts => Set<CatalogMembershipProduct>();
	public DbSet<CatalogUserDailySetting> UserDailySettings => Set<CatalogUserDailySetting>();

	// auth
	public DbSet<AuthUser> Users => Set<AuthUser>();
    public DbSet<AuthUserExternalLogin> UserExternalLogins => Set<AuthUserExternalLogin>();

    // sales
    public DbSet<SalesCreditCardApplication> CreditCardApplications => Set<SalesCreditCardApplication>();
    public DbSet<SalesMembershipSale> MembershipSales => Set<SalesMembershipSale>();
	public DbSet<Sale> Sales => Set<Sale>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===== catalog =====
        modelBuilder.Entity<CatalogIdentityProvider>(e =>
        {
            e.ToTable("IdentityProviders", "catalog");
            e.HasKey(x => x.IdentityProviderId);
            e.Property(x => x.ProviderCode).HasMaxLength(20).IsRequired().IsUnicode(false);
            e.Property(x => x.ProviderName).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.ProviderCode).IsUnique();
        });

        modelBuilder.Entity<CatalogStore>(e =>
        {
            e.ToTable("Stores", "catalog");
            e.HasKey(x => x.StoreId);
            e.HasIndex(x => x.StoreNumber).IsUnique();
        });

        modelBuilder.Entity<CatalogSaleStatus>(e =>
        {
            e.ToTable("SaleStatus", "catalog");
            e.HasKey(x => x.StatusId);
            e.Property(x => x.StatusCode).HasMaxLength(20).IsRequired().IsUnicode(false);
            e.Property(x => x.StatusName).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.StatusCode).IsUnique();
        });

        modelBuilder.Entity<CatalogCreditCardProduct>(e =>
        {
            e.ToTable("CreditCardProducts", "catalog");
            e.HasKey(x => x.CreditCardProductId);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired().IsUnicode(false);
            e.Property(x => x.ProductName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.ProductCode).IsUnique();
        });

        modelBuilder.Entity<CatalogMembershipProduct>(e =>
        {
            e.ToTable("MembershipProducts", "catalog");
            e.HasKey(x => x.MembershipProductId);
            e.Property(x => x.ProductCode).HasMaxLength(50).IsRequired().IsUnicode(false);
            e.Property(x => x.ProductName).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.ProductCode).IsUnique();
        });

		modelBuilder.Entity<CatalogUserDailySetting>(e =>
		{
			e.ToTable("UserDailySettings", "catalog");
			e.HasKey(x => x.Id);

			e.Property(x => x.SettingDate)
				.HasColumnType("date");

			e.Property(x => x.SalesGoalAmount)
				.HasPrecision(12, 2);

      e.Property(x => x.CreatedAt)
  .HasColumnType("timestamp with time zone");

      e.Property(x => x.UpdatedAt)
        .HasColumnType("timestamp with time zone");

      // FK -> Users (auth schema)
      e.HasOne(x => x.User)
				.WithMany()
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.Restrict);

			// FK -> Stores (catalog schema)
			e.HasOne(x => x.Store)
				.WithMany()
				.HasForeignKey(x => x.StoreId)
				.OnDelete(DeleteBehavior.Restrict);

			// Unique por día
			e.HasIndex(x => new { x.UserId, x.SettingDate })
				.IsUnique();
		});

		// ===== auth =====
		modelBuilder.Entity<AuthUser>(e =>
        {
            e.ToTable("Users", "auth");
            e.HasKey(x => x.UserId);
            e.Property(x => x.CreatedAtUtc).HasPrecision(0);
            e.Property(x => x.LastLoginAtUtc).HasPrecision(0);
            e.HasIndex(x => x.Email)
                .HasDatabaseName("IX_Users_Email")
                .HasFilter("\"Email\" IS NOT NULL");
        });

        modelBuilder.Entity<AuthUserExternalLogin>(e =>
        {
            e.ToTable("UserExternalLogins", "auth");
            e.HasKey(x => x.UserExternalLoginId);
            e.Property(x => x.ProviderSubject).HasMaxLength(300).IsRequired();
            e.Property(x => x.CreatedAtUtc).HasPrecision(0);

            e.HasOne(x => x.User)
                .WithMany(u => u.ExternalLogins)
                .HasForeignKey(x => x.UserId);

            e.HasOne(x => x.IdentityProvider)
                .WithMany(p => p.UserExternalLogins)
                .HasForeignKey(x => x.IdentityProviderId);

            e.HasIndex(x => new { x.IdentityProviderId, x.ProviderSubject }).IsUnique();
            e.HasIndex(x => x.UserId);
        });

        // ===== sales =====
        modelBuilder.Entity<SalesCreditCardApplication>(e =>
        {
            e.ToTable("CreditCardApplications", "sales");
            e.HasKey(x => x.CreditCardApplicationId);
            e.Property(x => x.SubmittedAtUtc).HasPrecision(0);

            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Store).WithMany().HasForeignKey(x => x.StoreId);
            e.HasOne(x => x.CreditCardProduct).WithMany().HasForeignKey(x => x.CreditCardProductId);
            e.HasOne(x => x.Status).WithMany(s => s.CreditCardApplications).HasForeignKey(x => x.StatusId);
        });

        modelBuilder.Entity<SalesMembershipSale>(e =>
        {
            e.ToTable("MembershipSales", "sales");
            e.HasKey(x => x.MembershipSaleId);
            e.Property(x => x.SoldAtUtc).HasPrecision(0);

            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Store).WithMany().HasForeignKey(x => x.StoreId);
            e.HasOne(x => x.MembershipProduct).WithMany().HasForeignKey(x => x.MembershipProductId);
            e.HasOne(x => x.Status).WithMany(s => s.MembershipSales).HasForeignKey(x => x.StatusId);
        });

		modelBuilder.Entity<Sale>(e =>
		{
			e.ToTable("Sales", "sales");
			e.HasKey(x => x.SaleId);

			e.Property(x => x.SaleId).ValueGeneratedOnAdd();

			e.Property(x => x.SaleDate)
				.HasColumnType("timestamp without time zone");

			e.Property(x => x.Subtotal)
				.HasPrecision(12, 2);

			e.Property(x => x.Tax)
				.HasPrecision(12, 2);

			// Computed column
			e.Property(x => x.Total)
				.HasPrecision(12, 2)
				.ValueGeneratedOnAddOrUpdate();

			e.Property(x => x.PaymentMethod)
				.HasMaxLength(30)
				.IsUnicode(false);

			e.Property(x => x.Notes)
				.HasMaxLength(500)
				.IsUnicode(true);

			e.Property(x => x.CreatedAt)
				.HasColumnType("timestamp without time zone");

			e.Property(x => x.UpdatedAt)
				.HasColumnType("timestamp without time zone");

			e.HasIndex(x => new { x.StoreId, x.SaleDate });
			e.HasIndex(x => new { x.UserId, x.SaleDate });
		});
	}
}


