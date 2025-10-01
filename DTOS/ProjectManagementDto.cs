namespace jep_construction_api.DTOS
{
    public class ProjectManagementDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Budget { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public double CompletionStatus { get; set; }

    }
}
