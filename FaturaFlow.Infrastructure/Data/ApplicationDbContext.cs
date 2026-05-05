using FaturaFlow.Domain.Entities;
using FaturaFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FaturaFlow.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.NIF).HasConversion(v => v == null ? null : v.Value, v => v == null ? null! : new PersonalId(v));
                entity.Property(c => c.Phone).HasConversion(v => v == null ? null : v.Value, v => v == null ? null! : new PhoneNumber(v));
                entity.Property(c => c.Email).HasConversion(v => v == null ? null : v.Value, v => v == null ? null! : new EmailAddress(v));
                entity.Property(c => c.ZipCode).HasConversion(v => v == null ? null : v.Value, v => v == null ? null! : new PostalCode(v));
            });

            // 2. Supplier
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.NIPC).HasConversion(v => v == null ? null : v.Value, v => v == null ? null : new PersonalId(v));
                entity.Property(s => s.Phone).HasConversion(v => v == null ? null : v.Value, v => v == null ? null : new PhoneNumber(v));
                entity.Property(s => s.Email).HasConversion(v => v == null ? null : v.Value, v => v == null ? null : new EmailAddress(v));
                entity.Property(s => s.ZipCode).HasConversion(v => v == null ? null : v.Value, v => v == null ? null : new PostalCode(v));
            });

            // 3. Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.PurchasePrice).HasConversion(v => v.Value, v => new Price(v));
                entity.Property(p => p.SalePrice).HasConversion(v => v.Value, v => new Price(v));
                entity.Property(p => p.VatRate).HasConversion(v => v.Value, v => new VatRate(v));
                entity.Property(p => p.PriceWithVat).HasConversion(v => v.Value, v => new Price(v));            });

            // 4. Invoice e InvoiceLine
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasMany(i => i.Lines).WithOne().HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InvoiceLine>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.UnitPrice).HasConversion(v => v.Value, v => new Price(v));
                entity.Property(l => l.VatRate).HasConversion(v => v.Value, v => new VatRate(v));
            });

            // 5. User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email)
                    .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : new EmailAddress(v))
                    .HasColumnName("Email");
            });

            // 6. Company
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.NIF).HasConversion(v => v == null ? null : v.Value, v => v == null ? null! : new PersonalId(v));
                entity.Property(c => c.Phone).HasConversion(v => v == null ? null : v.Value, v => v == null ? null : new PhoneNumber(v));
                entity.Property(c => c.Email).HasConversion(v => v == null ? null : v.Value, v => v == null ? null : new EmailAddress(v));
                entity.Property(c => c.ZipCode).HasConversion(v => v == null ? null : v.Value, v => v == null ? null : new PostalCode(v));
            });


            base.OnModelCreating(modelBuilder);
        }
    }
}