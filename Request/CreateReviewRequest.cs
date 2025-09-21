namespace jep_construction_api.Request
{
    public class CreateUpdateReviewRequest
    {
        public int? Id { get; set; }
        public long UserId { get; set; }
        public long ProjectManagementId { get; set; }
        public byte Rate { get; set; }
        public string ReviewDescription { get; set; }

    }
}
