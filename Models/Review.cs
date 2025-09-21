using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class Review
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ProjectManagementId { get; set; }
        public byte Rate { get; set; }
        public string ReviewDescription { get; set; } = null!;
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual ProjectManagement ProjectManagement { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
