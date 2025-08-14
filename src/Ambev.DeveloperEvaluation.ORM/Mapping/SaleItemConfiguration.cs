using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        
        builder.HasKey(si => si.Id);
        
        builder.Property(si => si.SaleId)
            .IsRequired();
            
        builder.Property(si => si.ProductId)
            .IsRequired();
            
        builder.Property(si => si.ProductDescription)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(si => si.Quantity)
            .IsRequired();
            
        builder.Property(si => si.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();
            
        builder.Property(si => si.Discount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
            
        builder.Property(si => si.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasIndex(si => si.SaleId)
            .HasDatabaseName("IX_SaleItems_SaleId");
            
        builder.HasIndex(si => si.ProductId)
            .HasDatabaseName("IX_SaleItems_ProductId");
    }
}