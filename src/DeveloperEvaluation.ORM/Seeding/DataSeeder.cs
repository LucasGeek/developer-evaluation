using DeveloperEvaluation.Domain.Entities;
using DeveloperEvaluation.Domain.Enums;
using DeveloperEvaluation.Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeveloperEvaluation.ORM.Seeding;

public class DataSeeder : IDataSeeder
{
    private readonly DefaultContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(DefaultContext context, IPasswordHasher passwordHasher, ILogger<DataSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding...");

            await SeedUsersAsync();
            await SeedProductsAsync();
            await SeedCartsAsync();
            await SeedSalesAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during data seeding");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist, skipping user seeding");
            return;
        }

        _logger.LogInformation("Seeding users...");

        var users = new[]
        {
            new User()
            {
                Email = "admin@developereval.com",
                Username = "admin",
                Password = _passwordHasher.HashPassword("Admin123!"),
                Phone = "+5511999999999",
                Role = UserRole.Admin,
                Status = UserStatus.Active
            },
            new User()
            {
                Email = "manager@developereval.com",
                Username = "manager",
                Password = _passwordHasher.HashPassword("Manager123!"),
                Phone = "+5511888888888",
                Role = UserRole.Manager,
                Status = UserStatus.Active
            },
            new User()
            {
                Email = "customer1@developereval.com",
                Username = "customer1",
                Password = _passwordHasher.HashPassword("Customer123!"),
                Phone = "+5511777777777",
                Role = UserRole.Customer,
                Status = UserStatus.Active
            },
            new User()
            {
                Email = "customer2@developereval.com",
                Username = "customer2",
                Password = _passwordHasher.HashPassword("Customer123!"),
                Phone = "+5511666666666",
                Role = UserRole.Customer,
                Status = UserStatus.Active
            },
            new User()
            {
                Email = "customer3@developereval.com",
                Username = "customer3",
                Password = _passwordHasher.HashPassword("Customer123!"),
                Phone = "+5511555555555",
                Role = UserRole.Customer,
                Status = UserStatus.Active
            }
        };

        await _context.Users.AddRangeAsync(users);
        _logger.LogInformation("Seeded {UserCount} users", users.Length);
    }

    private async Task SeedProductsAsync()
    {
        if (await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Products already exist, skipping product seeding");
            return;
        }

        _logger.LogInformation("Seeding products...");

        var products = new[]
        {
            // Beverages
            new Product("Budweiser Lager Beer", 8.99m, "Premium American lager beer with a crisp, refreshing taste", "beverages", "https://cdn.developereval.com/products/budweiser.jpg"),
            new Product("Stella Artois Premium Lager", 9.49m, "Belgian premium lager with a distinctive hop character", "beverages", "https://cdn.developereval.com/products/stella.jpg"),
            new Product("Corona Extra", 7.99m, "Light, crisp Mexican beer perfect for any occasion", "beverages", "https://cdn.developereval.com/products/corona.jpg"),
            new Product("Brahma Chopp", 5.99m, "Traditional Brazilian beer with authentic flavor", "beverages", "https://cdn.developereval.com/products/brahma.jpg"),
            new Product("Antarctica Pilsen", 5.49m, "Brazilian pilsen beer with smooth taste", "beverages", "https://cdn.developereval.com/products/antarctica.jpg"),

            // Snacks
            new Product("Doritos Nacho Cheese", 4.99m, "Classic nacho cheese flavored tortilla chips", "snacks", "https://cdn.developereval.com/products/doritos-nacho.jpg"),
            new Product("Lay's Classic Potato Chips", 3.99m, "Original crispy potato chips with sea salt", "snacks", "https://cdn.developereval.com/products/lays-classic.jpg"),
            new Product("Pringles Original", 5.99m, "Stackable potato crisps with original flavor", "snacks", "https://cdn.developereval.com/products/pringles-original.jpg"),
            new Product("Cheetos Crunchy", 4.49m, "Crunchy cheese-flavored corn puffs", "snacks", "https://cdn.developereval.com/products/cheetos.jpg"),

            // Soft Drinks
            new Product("Guaraná Antarctica", 3.99m, "Brazilian guaraná soft drink with unique flavor", "soft drinks", "https://cdn.developereval.com/products/guarana.jpg"),
            new Product("Pepsi Cola 350ml", 2.99m, "Classic cola soft drink", "soft drinks", "https://cdn.developereval.com/products/pepsi.jpg"),
            new Product("H2OH Limão", 3.49m, "Flavored water with lemon taste", "soft drinks", "https://cdn.developereval.com/products/h2oh.jpg"),

            // Premium Beverages
            new Product("Beck's German Pilsener", 12.99m, "Authentic German pilsener with premium quality", "premium beverages", "https://cdn.developereval.com/products/becks.jpg"),
            new Product("Leffe Belgian Blonde", 15.99m, "Premium Belgian abbey beer", "premium beverages", "https://cdn.developereval.com/products/leffe.jpg"),
            new Product("Hoegaarden White Beer", 13.99m, "Belgian wheat beer with coriander and orange peel", "premium beverages", "https://cdn.developereval.com/products/hoegaarden.jpg"),

            // Energy Drinks
            new Product("Red Bull Energy Drink", 6.99m, "Energy drink that gives you wings", "energy drinks", "https://cdn.developereval.com/products/redbull.jpg"),
            new Product("Monster Energy Green", 5.99m, "High-energy drink for active lifestyle", "energy drinks", "https://cdn.developereval.com/products/monster.jpg")
        };

        await _context.Products.AddRangeAsync(products);
        
        var random = new Random();
        foreach (var product in products)
        {
            var rate = Math.Round(3.5 + random.NextDouble() * 1.5, 1); // 3.5 to 5.0
            var count = random.Next(100, 2500);
            product.UpdateRating(rate, count);
        }
        
        _logger.LogInformation("Seeded {ProductCount} products", products.Length);
    }

    private async Task SeedCartsAsync()
    {
        if (await _context.Carts.AnyAsync())
        {
            _logger.LogInformation("Carts already exist, skipping cart seeding");
            return;
        }

        _logger.LogInformation("Seeding carts...");

        var users = await _context.Users.Where(u => u.Role == UserRole.Customer).ToListAsync();
        var products = await _context.Products.Take(10).ToListAsync();

        if (!users.Any() || !products.Any())
        {
            _logger.LogWarning("No users or products found for cart seeding");
            return;
        }

        var carts = new List<Cart>();
        var random = new Random();

        foreach (var user in users.Take(3))
        {
            var cart = new Cart(user.Id);
            
            var cartProducts = products.OrderBy(x => random.Next()).Take(random.Next(2, 6)).ToList();
            foreach (var product in cartProducts)
            {
                cart.AddProduct(product.Id, random.Next(1, 5));
            }

            carts.Add(cart);
        }

        await _context.Carts.AddRangeAsync(carts);
        _logger.LogInformation("Seeded {CartCount} carts", carts.Count);
    }

    private async Task SeedSalesAsync()
    {
        if (await _context.Sales.AnyAsync())
        {
            _logger.LogInformation("Sales already exist, skipping sales seeding");
            return;
        }

        _logger.LogInformation("Seeding sales...");

        var customers = await _context.Users.Where(u => u.Role == UserRole.Customer).ToListAsync();
        var products = await _context.Products.ToListAsync();

        if (!customers.Any() || !products.Any())
        {
            _logger.LogWarning("No customers or products found for sales seeding");
            return;
        }

        var sales = new List<Sale>();
        var random = new Random();
        var branches = new[]
        {
            new { Id = Guid.NewGuid(), Description = "São Paulo - Centro" },
            new { Id = Guid.NewGuid(), Description = "Rio de Janeiro - Copacabana" },
            new { Id = Guid.NewGuid(), Description = "Belo Horizonte - Savassi" },
            new { Id = Guid.NewGuid(), Description = "Brasília - Asa Norte" },
            new { Id = Guid.NewGuid(), Description = "Salvador - Pelourinho" }
        };

        for (int i = 0; i < 15; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            var branch = branches[random.Next(branches.Length)];
            
            var sale = new Sale(
                $"BRANCH{branch.Id.ToString("N")[..8].ToUpper()}-{(i + 1):D4}",
                DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                customer.Id,
                customer.Username,
                branch.Id,
                branch.Description
            );

            var saleProducts = products.OrderBy(x => random.Next()).Take(random.Next(1, 5)).ToList();
            foreach (var product in saleProducts)
            {
                var quantity = random.Next(1, 15); 
                var saleItem = new SaleItem(
                    sale.Id,
                    product.Id,
                    product.Title,
                    quantity,
                    product.Price
                );
                
                saleItem.ApplyDiscount();
                sale.AddItem(saleItem);
            }

            if (random.NextDouble() < 0.15)
            {
                sale.Cancel();
            }

            sales.Add(sale);
        }

        await _context.Sales.AddRangeAsync(sales);
        _logger.LogInformation("Seeded {SaleCount} sales", sales.Count);
    }
}