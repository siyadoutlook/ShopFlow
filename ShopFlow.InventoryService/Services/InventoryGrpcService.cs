using Grpc.Core;
using ShopFlow.InventoryService.Data;
using ShopFlow.Shared.Protos;
using static ShopFlow.Shared.Protos.InventoryService;

namespace ShopFlow.InventoryService.Services;

public class InventoryGrpcService : InventoryServiceBase
{
    public override Task<StockResponse> CheckStock(StockRequest request, ServerCallContext context)
    {
        var product = ProductStore.Products.FirstOrDefault(p => p.Id == request.ProductId);

        if (product == null)
        {
            return Task.FromResult(new StockResponse
            {
                IsAvailable = false,
                AvailableStock = 0
            });
        }

        return Task.FromResult(new StockResponse
        {
            IsAvailable = product.Stock >= request.Quantity,
            AvailableStock = product.Stock
        });
    }
}
