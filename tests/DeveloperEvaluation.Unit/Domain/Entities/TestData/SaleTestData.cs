using DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .CustomInstantiator(f => new Sale(
            f.Random.AlphaNumeric(10),
            f.Date.Recent(),
            f.Random.Guid(),
            f.Company.CompanyName(),
            f.Random.Guid(),
            f.Address.City()
        ));

    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .CustomInstantiator(f => new SaleItem(
            f.Random.Guid(),
            f.Random.Guid(),
            f.Commerce.ProductName(),
            f.Random.Int(1, 20),
            f.Random.Decimal(1, 100)
        ));

    public static Sale GenerateValidSale()
    {
        return SaleFaker.Generate();
    }

    public static List<Sale> GenerateValidSales(int count = 3)
    {
        return SaleFaker.Generate(count);
    }

    public static SaleItem GenerateValidSaleItem(Guid? saleId = null)
    {
        var item = SaleItemFaker.Generate();
        if (saleId.HasValue)
        {
            var newItem = new SaleItem(saleId.Value, item.ProductId, item.ProductDescription, item.Quantity, item.UnitPrice);
            return newItem;
        }
        return item;
    }

    public static SaleItem GenerateItemWithQuantity(int quantity, Guid? saleId = null)
    {
        var baseItem = GenerateValidSaleItem(saleId);
        return new SaleItem(
            baseItem.SaleId,
            baseItem.ProductId,
            baseItem.ProductDescription,
            quantity,
            baseItem.UnitPrice
        );
    }

    public static Sale GenerateSaleWithItems(int itemCount = 3)
    {
        var sale = GenerateValidSale();
        for (int i = 0; i < itemCount; i++)
        {
            var item = GenerateValidSaleItem(sale.Id);
            sale.AddItem(item);
        }
        return sale;
    }
}