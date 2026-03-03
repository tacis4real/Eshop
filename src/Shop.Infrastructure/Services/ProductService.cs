using Microsoft.EntityFrameworkCore;
using Shop.Application.DTOs;
using Shop.Application.Interfaces;
using Shop.Infrastructure.Persistence;

namespace Shop.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context) => _context = context;

    public async Task<List<ProductDto>> GetAllAsync() =>
        await _context.Products
            .Where(p => !p.IsDeleted)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.ImageUrl, p.StockQuantity))
            .ToListAsync();
}
