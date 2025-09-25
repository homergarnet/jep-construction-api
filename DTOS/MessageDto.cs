namespace jep_construction_api.DTOS
{
    public class MessageDto
    {
        public long UserId { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string Message { get; set; }
        public string IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }
}
