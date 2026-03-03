using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shop.Domain.Entities;
using Shop.Infrastructure.Identity;

namespace Shop.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                new() { Name = "Wireless Headphones", Description = "Over-ear noise-cancelling bluetooth headphones.", Price = 79.99m, StockQuantity = 50, ImageUrl = "https://example.com/images/headphones.jpg" },
                new() { Name = "Mechanical Keyboard", Description = "Compact TKL mechanical keyboard with RGB backlighting.", Price = 59.99m, StockQuantity = 30, ImageUrl = "https://example.com/images/keyboard.jpg" },
                new() { Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI, USB 3.0 and SD card reader.", Price = 34.99m, StockQuantity = 100, ImageUrl = "https://example.com/images/hub.jpg" },
                new() { Name = "Webcam HD 1080p", Description = "Full HD webcam with built-in microphone and auto-focus.", Price = 49.99m, StockQuantity = 75, ImageUrl = "https://example.com/images/webcam.jpg" },
                new() { Name = "Mouse Pad XL", Description = "Extra-large desk mouse pad with anti-slip rubber base.", Price = 14.99m, StockQuantity = 200, ImageUrl = "https://example.com/images/mousepad.jpg" },
                new() { Name = "LED Desk Lamp", Description = "Dimmable LED desk lamp with USB charging port.", Price = 24.99m, StockQuantity = 60, ImageUrl = "https://example.com/images/lamp.jpg" },
                new() { Name = "Laptop Stand", Description = "Adjustable aluminium laptop stand for ergonomic working.", Price = 39.99m, StockQuantity = 45, ImageUrl = "https://example.com/images/laptopstand.jpg" },
                new() { Name = "Portable SSD 1TB", Description = "Compact 1TB portable solid-state drive with USB-C.", Price = 109.99m, StockQuantity = 25, ImageUrl = "https://example.com/images/ssd.jpg" },
                new() { Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with long battery life.", Price = 29.99m, StockQuantity = 80, ImageUrl = "https://example.com/images/mouse.jpg" },
                new() { Name = "Smart Plug", Description = "Wi-Fi enabled smart plug with energy monitoring.", Price = 12.99m, StockQuantity = 150, ImageUrl = "https://example.com/images/smartplug.jpg" }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        await SeedUsersAsync(userManager);
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // WARNING: These are default seed credentials for development/demo. Change them before deploying to production.
        var seedUsers = new[]
        {
            new { Email = "admin@shop.com", FirstName = "Admin", LastName = "User", Password = "P@ssw0rd!" },
            new { Email = "user@shop.com",  FirstName = "Test",  LastName = "User", Password = "P@ssw0rd!" }
        };

        foreach (var seed in seedUsers)
        {
            if (await userManager.FindByEmailAsync(seed.Email) is not null)
                continue;

            var user = new ApplicationUser
            {
                UserName       = seed.Email,
                Email          = seed.Email,
                FirstName      = seed.FirstName,
                LastName       = seed.LastName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, seed.Password);
            if (result.Succeeded)
                Console.WriteLine($"[DbInitializer] Seeded user: {seed.Email}");
            else
                Console.WriteLine($"[DbInitializer] Failed to seed user {seed.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}
