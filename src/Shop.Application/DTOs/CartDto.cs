namespace Shop.Application.DTOs;

public record CartDto(
    long Id,
    bool IsActive,
    decimal Total,
    List<CartItemDto> Items);
