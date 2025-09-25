using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IMessageService
    {
        MessageResponse CreateMessage(CreateMessageRequest req);
        MessageResponse GetMessageList(string keyword, long userId, int page, int pageSize);
    }
}
