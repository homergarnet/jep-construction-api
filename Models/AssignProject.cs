using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class AssignProject
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ProjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual ProjectManagement Project { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
