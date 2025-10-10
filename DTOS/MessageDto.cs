namespace jep_construction_api.DTOS
{
    public class MessageDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string Message { get; set; }
        public string ProfileImage { get; set; }
        public string IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
    }

    public class ConvoDto
    {
        public long ConvoUserId { get; set; }
        public string ConvoImage { get; set; }
        public string ConvoName { get; set; }
        public int UnreadCount { get; set; }
        public string LastMessage { get; set; }
    }
}
