using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class Inventory
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string ItemName { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int Quantity { get; set; }
        public string UnitOfMeasure { get; set; } = null!;
        public int ReOrderLevel { get; set; }
        public int ReOrderQuantity { get; set; }
        public string Description { get; set; } = null!;
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
