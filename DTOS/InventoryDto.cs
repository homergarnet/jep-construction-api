namespace jep_construction_api.DTOS
{
    public class InventoryDto
    {
        public long? Id { get; set; }
        public long UserId { get; set; }
        public string ClientName { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        public int ReOrderLevel { get; set; }
        public int ReOrderQuantity { get; set; }
        public string Description { get; set; }
    }
}
