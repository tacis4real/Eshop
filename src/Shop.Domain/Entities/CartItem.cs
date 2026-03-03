using Shop.Domain.Common;

namespace Shop.Domain.Entities;

public class CartItem : BaseEntity
{
    public long CartId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
