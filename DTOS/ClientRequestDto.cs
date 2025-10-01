namespace jep_construction_api.DTOS
{
    public class ClientRequestDto
    {
        public long? Id { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Message { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }
}
