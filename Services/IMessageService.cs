using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IMessageService
    {
        MessageResponse CreateMessage(CreateMessageRequest req);
        ConvoResponse GetConvoRowList(long? userId, int page, int pageSize);
        MessageResponse GetMessageList(string keyword, long userId, int page, int pageSize);
        EmployeeListResponse GetMessageUserList(string keyword, long userId, int page, int pageSize);
        MessageResponse SetReadById(long senderId, long userId);
    }
}
