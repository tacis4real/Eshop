using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;

namespace Shop.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService) => _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetProducts() =>
        Ok(await _productService.GetAllAsync());
}
