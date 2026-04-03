using MassTransit;
using ShopFlow.Shared.Events;

namespace ShopFlow.NotificationService.Consumer;

public class OrderConfirmedConsumer(ILogger<OrderConfirmedConsumer> _logger) : IConsumer<OrderConfirmed>
{
    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var message = context.Message;

        _logger.LogInformation($"Sending confirmation email for order {message.OrderId}. " +
                          $"Product: {message.ProductId}, " +
                          $"Quantity: {message.Quantity}, " +
                          $"Confirmed at: {message.ConfirmedAt}");
    }
}
