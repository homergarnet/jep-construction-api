using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class EmployeePayslip
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string? Period { get; set; }
        public decimal? RegBasic { get; set; }
        public decimal? Tardy { get; set; }
        public decimal? Undertime { get; set; }
        public decimal? Absent { get; set; }
        public decimal? Overtime { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual User Employee { get; set; } = null!;
    }
}
