using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IClientRequestService
    {
        ClientRequestResponse CreateClientRequest(CreateUpdateClientRequest req);
        ClientRequestResponse GetClientRequestById(long id);
        ClientRequestResponse GetClientRequestList(string keyword, int page, int pageSize);
        ClientRequestResponse SoftDeleteClientRequestById(string id);
        Task<ClientRequestResponse> SendEmail(EmailRequest req);
    }
}
