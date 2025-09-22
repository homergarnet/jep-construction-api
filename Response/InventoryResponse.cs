using jep_construction_api.DTOS;

namespace jep_construction_api.Response
{
    public class InventoryResponse
    {
        public List<InventoryDto> InventoryList { get; set; }
        public long TotalRecords { get; set; }
        public bool IsSuccess { get; set; }
        public string ApiMessage { get; set; }
    }
}
