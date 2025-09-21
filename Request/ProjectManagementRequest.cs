using System.ComponentModel.DataAnnotations;

namespace jep_construction_api.Request
{
    public class CreateUpdateProjectManagementRequest
    {
        public long? Id { get; set; }
        public long UserId { get; set; }
        [Required]
        public string ProjectName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public double CompletionStatus { get; set; }
        public DateTime? DateTimeUpdated { get; set; }
    }
}
