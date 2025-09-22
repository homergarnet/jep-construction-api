using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IInventoryService
    {
        InventoryResponse CreateInventory(CreateUpdateInventoryRequest req);
        InventoryResponse GetInventoryById(long id);
        InventoryResponse GetInventoryList(string keyword, long? userId, int page, int pageSize);
        InventoryResponse SoftDeleteInventoryById(string id);
        InventoryResponse UpdateInventory(CreateUpdateInventoryRequest req);
    }
}
