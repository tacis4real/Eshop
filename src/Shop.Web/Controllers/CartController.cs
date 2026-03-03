using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Application.Requests;

namespace Shop.Web.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService) => _cartService = cartService;

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _cartService.GetOrCreateCartAsync(userId.Value));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        if (request.Quantity is < 1 or > 999)
            return BadRequest(new { Message = "Quantity must be between 1 and 999." });

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            return Ok(await _cartService.AddItemAsync(userId.Value, request));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { ex.Message });
        }
    }

    [HttpPut("items/{itemId:long}")]
    public async Task<IActionResult> UpdateItem(long itemId, [FromBody] UpdateCartItemRequest request)
    {
        if (request.Quantity is < 0 or > 999)
            return BadRequest(new { Message = "Quantity must be between 0 and 999. Use 0 to remove the item." });

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var cart = await _cartService.UpdateItemAsync(userId.Value, itemId, request);
        return cart is null ? NotFound(new { Message = "Cart item not found." }) : Ok(cart);
    }

    [HttpDelete("items/{itemId:long}")]
    public async Task<IActionResult> RemoveItem(long itemId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var cart = await _cartService.RemoveItemAsync(userId.Value, itemId);
        return cart is null ? NotFound(new { Message = "Cart item not found." }) : Ok(cart);
    }

    private long? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
        return long.TryParse(value, out var id) ? id : null;
    }
}
