namespace jep_construction_api.DTOS
{
    public class ReviewDto
    {
        public long Id { get; set; }
        public string ProjectName { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Rate { get; set; }
        public string ReviewDescription { get; set; }
        public DateTime DateTimeCreated { get; set; }

    }
}
