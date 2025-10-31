namespace jep_construction_api.Request
{
    public class AssignProjectCreateUpdateRequest
    {
        public long? Id { get; set; }
        public long UserId { get; set; }
        public long ProjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
