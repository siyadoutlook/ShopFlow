using ShopFlow.InventoryService.Models;

namespace ShopFlow.InventoryService.Data;

public static class ProductStore
{
    public static List<Product> Products = new()
    {
        new Product { Id = 1, Name = "Laptop",   Price = 999.99m, Stock = 10 },
        new Product { Id = 2, Name = "Mouse",    Price = 29.99m,  Stock = 50 },
        new Product { Id = 3, Name = "Keyboard", Price = 49.99m,  Stock = 30 },
    };
}
