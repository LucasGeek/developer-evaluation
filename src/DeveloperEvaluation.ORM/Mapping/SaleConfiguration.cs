using DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(s => s.Date)
            .IsRequired();
            
        builder.Property(s => s.CustomerId)
            .IsRequired();
            
        builder.Property(s => s.CustomerDescription)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(s => s.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(s => s.BranchId)
            .IsRequired();
            
        builder.Property(s => s.BranchDescription)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(s => s.Cancelled)
            .HasDefaultValue(false);
            
        builder.Property(s => s.CancelledAt)
            .IsRequired(false);
            
        builder.Property(s => s.CreatedAt)
            .IsRequired();
            
        builder.Property(s => s.UpdatedAt)
            .IsRequired();
            
        builder.Property(s => s.RowVersion)
            .IsRequired(false)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(s => new { s.SaleNumber, s.BranchId })
            .IsUnique()
            .HasDatabaseName("IX_Sales_SaleNumber_BranchId");
            
        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}