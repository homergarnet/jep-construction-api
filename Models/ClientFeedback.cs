using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class ClientFeedback
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Feedback { get; set; } = null!;
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
