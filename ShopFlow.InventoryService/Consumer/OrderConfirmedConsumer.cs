using MassTransit;
using ShopFlow.InventoryService.Data;
using ShopFlow.Shared.Events;

namespace ShopFlow.InventoryService.Consumer;

public class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
{
    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var message = context.Message;

        var product = ProductStore.Products.FirstOrDefault(x => x.Id == message.ProductId);

        if (product == null)
        {
            Console.WriteLine($"Product {message.ProductId} not found");
            return;
        }

        product.Stock -= message.Quantity;

        Console.WriteLine($"Stock reduced for product {product.Name}. " +
                          $"Remaining: {product.Stock}");
    }
}
