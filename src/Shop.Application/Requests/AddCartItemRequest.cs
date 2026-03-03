namespace Shop.Application.Requests;

public record AddCartItemRequest(long ProductId, int Quantity);
