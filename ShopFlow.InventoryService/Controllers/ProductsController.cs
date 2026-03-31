using Microsoft.AspNetCore.Mvc;
using ShopFlow.InventoryService.Data;
using ShopFlow.InventoryService.Models;

namespace ShopFlow.InventoryService.Controllers;

[Route("inventory")]
[ApiController]
public class ProductsController : ControllerBase
{
    [HttpGet("products")]
    public ActionResult<List<Product>> GetAll()
    {
        return Ok(ProductStore.Products);
    }

    [HttpGet("products/{id}")]
    public ActionResult<Product> GetById()
    {
        var product = ProductStore.Products.FirstOrDefault();

        if (product == null) return NotFound();

        return Ok(product);
    }
}
