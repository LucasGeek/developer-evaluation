using DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeveloperEvaluation.ORM.Mapping;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.UserId)
            .IsRequired();
            
        builder.Property(c => c.Date)
            .IsRequired();
            
        builder.OwnsMany(c => c.Products, products =>
        {
            products.ToTable("CartItems");
            products.WithOwner().HasForeignKey("CartId");
            products.HasKey("CartId", "ProductId");
            
            products.Property(p => p.ProductId)
                .IsRequired();
                
            products.Property(p => p.Quantity)
                .IsRequired();
        });
        
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.Date);
    }
}