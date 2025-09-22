using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class ClientRequest
    {
        public long Id { get; set; }
        public string ProjectName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }
}
