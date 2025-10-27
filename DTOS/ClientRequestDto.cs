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
        public bool HasReply { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }

    public class SmtpSettings
    {
        public string SmtpServer { get; set; } = "";
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = "";
        public string SmtpPassword { get; set; } = "";
    }
}
