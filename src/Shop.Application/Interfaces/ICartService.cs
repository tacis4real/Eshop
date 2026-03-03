using Shop.Application.DTOs;
using Shop.Application.Requests;

namespace Shop.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetOrCreateCartAsync(long userId);
    Task<CartDto> AddItemAsync(long userId, AddCartItemRequest request);
    Task<CartDto?> UpdateItemAsync(long userId, long itemId, UpdateCartItemRequest request);
    Task<CartDto?> RemoveItemAsync(long userId, long itemId);
}
