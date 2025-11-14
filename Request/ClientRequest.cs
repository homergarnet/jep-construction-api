namespace jep_construction_api.Request
{
    public class CreateUpdateClientRequest
    {
        public long? Id { get; set; }
        public string? ProjectName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string Message { get; set; }
    }

    public class EmailRequest
    {
        public long Id { get; set; }
        public string To { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
    }
}
