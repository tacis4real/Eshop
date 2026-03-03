using Shop.Application.DTOs;

namespace Shop.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
}
