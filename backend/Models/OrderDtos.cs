namespace backend.Models;

public record CreateOrderItemDto(
    Guid ProductId,
    int Qty
);

public record CreateOrderRequest(
    List<CreateOrderItemDto> Items
);