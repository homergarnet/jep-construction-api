using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class ProjectManagement
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Status { get; set; } = null!;
        public decimal? Budget { get; set; }
        public byte[]? Location { get; set; }
        public string? Description { get; set; }
        public double? CompletionStatus { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
