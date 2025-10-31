using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IAssignProjectService
    {
        AssignProjectResponse CreateAssignProject(AssignProjectCreateUpdateRequest req);
        AssignProjectResponse GetAssignProjectList(string keyword, long? userId, int page, int pageSize);
        AssignProjectResponse GetAssignProjectById(long id);
        AssignProjectResponse UpdateAssignProject(AssignProjectCreateUpdateRequest req);
        AssignProjectResponse SoftDeleteAssignProjectById(string id);
    }
}
