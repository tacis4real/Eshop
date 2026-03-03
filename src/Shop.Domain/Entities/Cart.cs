using Shop.Domain.Common;

namespace Shop.Domain.Entities;

public class Cart : BaseEntity
{
    public long UserId { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
