using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopFlow.OrderService.Data;
using ShopFlow.OrderService.Models;
using ShopFlow.Shared.Events;
using ShopFlow.Shared.Protos;

namespace ShopFlow.OrderService.Controllers;

[Route("orders")]
[ApiController]
public class OrderController(InventoryService.InventoryServiceClient inventoryClient, OrderDbContext dbContext,
    IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Order>>> GetAll()
    {
        return Ok(await dbContext.Orders.ToListAsync());
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

        order.OrderStatus = OrderStatus.Confirmed;
        await dbContext.Orders.AddAsync(order);
        await publishEndpoint.Publish(new OrderConfirmed
        {
            OrderId = order.Id,
            Quantity = order.Quantity,
            ProductId = order.ProductId,
            ConfirmedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetById(int id)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        return Ok(order);
    }
}
