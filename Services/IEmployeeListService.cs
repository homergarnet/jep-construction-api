using jep_construction_api.DTOS;
using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IEmployeeListService
    {

        EmployeeListResponse CreateEmployee(CreateUpdateEmployeeRequest createEmployeeReq);
        EmployeeListResponse GetEmployeeById(long id);
        EmployeeListResponse GetEmployeeList(string keyword, string? accountType, int page, int pageSize);
        EmployeeListResponse SoftDeleteEmployeeById(string id);
        EmployeeListResponse UpdateEmployee(CreateUpdateEmployeeRequest updateEmployeeReq);
    }
}
