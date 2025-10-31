using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class ProjectManagement
    {
        public ProjectManagement()
        {
            AssignProjects = new HashSet<AssignProject>();
            Reviews = new HashSet<Review>();
        }

        public long Id { get; set; }
        public long UserId { get; set; }
        public string ProjectName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Budget { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public double? CompletionStatus { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<AssignProject> AssignProjects { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
