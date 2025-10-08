using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class Message
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long SenderId { get; set; }
        public long? ReceiverId { get; set; }
        public string Message1 { get; set; } = null!;
        public bool IsRead { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual User Sender { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
