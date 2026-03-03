namespace Shop.Application.DTOs;

public record ProductDto(
    long Id,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    int StockQuantity);
