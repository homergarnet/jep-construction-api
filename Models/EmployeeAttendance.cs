using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class EmployeeAttendance
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string Location { get; set; } = null!;
        public DateTime TimeInOut { get; set; }
        public string TimeInOutType { get; set; } = null!;
        public string TimeInOutImage { get; set; } = null!;
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual User Employee { get; set; } = null!;
    }
}
