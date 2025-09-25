namespace jep_construction_api.Request
{
    public class CreateMessageRequest
    {
        public long UserId { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string Message { get; set; }

    }
}
