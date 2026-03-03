namespace Shop.Application.DTOs;

public record CartItemDto(
    long Id,
    long ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
