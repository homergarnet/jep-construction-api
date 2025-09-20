using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IProjectManagementService
    {
        ProjectManagementResponse CreateProjectManagement(CreateUpdateProjectManagementRequest req);
        ProjectManagementResponse GetProjectManagementById(long id);
        ProjectManagementResponse GetProjectManagementList(string keyword, long? userId, int page, int pageSize);
        ProjectManagementResponse SoftDeleteProjectManagementById(string id);
        ProjectManagementResponse UpdateProjectManagement(CreateUpdateProjectManagementRequest req);
    }
}
