namespace Application.Dtos.Request;

public record OrderItemDto(int OrderId, int ProductId, int Quantity) { }
