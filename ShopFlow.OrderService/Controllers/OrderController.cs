using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using ShopFlow.OrderService.Data;
using ShopFlow.OrderService.Models;
using ShopFlow.Shared.Protos;

namespace ShopFlow.OrderService.Controllers;

[Route("orders")]
[ApiController]
public class OrderController(InventoryService.InventoryServiceClient inventoryClient) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Order>> GetAll()
    {
        return Ok(OrderStore.Orders);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(Order order)
    {
        var stockResponse = await inventoryClient.CheckStockAsync(new StockRequest()
        {
            ProductId = order.ProductId,
            Quantity = order.Quantity
        });

        if (!stockResponse.IsAvailable)
        {
            return BadRequest(new
            {
                message = $"Insufficient stock. Available: {stockResponse.AvailableStock}"
            });
        }

        order.Id = OrderStore.Orders.Count + 1;
        order.OrderStatus = OrderStatus.Pending;
        OrderStore.Orders.Add(order);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public ActionResult<Order> GetById(int id)
    {
        var order = OrderStore.Orders.FirstOrDefault(o => o.Id == id);

        if (order is null)
            return NotFound();

        return Ok(order);
    }
}
