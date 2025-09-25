using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceResponse> CreateTimeInOut(CreateTimeInOutRequest req);
        AttendanceResponse GetAttendanceList(string keyword, long? userId, int page, int pageSize);
        AttendanceResponse SoftDeleteAttendanceById(string id);
        AttendanceResponse UpdateAttendance(UpdateAttendanceRequest req);
    }
}
