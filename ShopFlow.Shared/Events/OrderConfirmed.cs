namespace ShopFlow.Shared.Events;

public class OrderConfirmed
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime ConfirmedAt { get; set; }
}
