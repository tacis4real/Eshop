using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Application.Requests;
using Shop.Domain.Entities;
using Shop.Infrastructure.Persistence;

namespace Shop.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context) => _context = context;

    public async Task<CartDto> GetOrCreateCartAsync(long userId)
    {
        var cart = await GetActiveCartAsync(userId);
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            cart = (await GetActiveCartAsync(userId))!;
        }
        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(long userId, AddCartItemRequest request)
    {
        var product = await _context.Products
            .Where(p => p.Id == request.ProductId && !p.IsDeleted)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Product not found.");

        var cart = await GetActiveCartAsync(userId);
        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            cart = (await GetActiveCartAsync(userId))!;
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem is not null)
        {
            existingItem.Quantity = Math.Min(existingItem.Quantity + request.Quantity, 999);
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                UnitPrice = product.Price
            });
        }

        await _context.SaveChangesAsync();
        cart = (await GetActiveCartAsync(userId))!;
        return MapToDto(cart);
    }

    public async Task<CartDto?> UpdateItemAsync(long userId, long itemId, UpdateCartItemRequest request)
    {
        var cart = await GetActiveCartAsync(userId);
        if (cart is null) return null;

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return null;

        if (request.Quantity <= 0)
            _context.CartItems.Remove(item);
        else
            item.Quantity = request.Quantity;

        await _context.SaveChangesAsync();
        cart = (await GetActiveCartAsync(userId))!;
        return MapToDto(cart);
    }

    public async Task<CartDto?> RemoveItemAsync(long userId, long itemId)
    {
        var cart = await GetActiveCartAsync(userId);
        if (cart is null) return null;

        var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is null) return null;

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        cart = (await GetActiveCartAsync(userId))!;
        return MapToDto(cart);
    }

    private async Task<Cart?> GetActiveCartAsync(long userId) =>
        await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive && !c.IsDeleted);

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items
            .Select(i => new CartItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity))
            .ToList();

        return new CartDto(cart.Id, cart.IsActive, items.Sum(i => i.LineTotal), items);
    }
}
