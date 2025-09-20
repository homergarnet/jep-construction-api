using jep_construction_api.DTOS;

namespace jep_construction_api.Response
{
    public class ProjectManagementResponse
    {
        public List<ProjectManagementDto> ProjectManagementList { get; set; }
        public long TotalRecords { get; set; }
        public bool IsSuccess { get; set; }
        public string ApiMessage { get; set; }
    }
}
