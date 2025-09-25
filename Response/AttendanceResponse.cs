using jep_construction_api.DTOS;
using jep_construction_api.Request;

namespace jep_construction_api.Response
{
    public class AttendanceResponse
    {
        public List<AttendanceDto> AttendanceList { get; set; }
        public long TotalRecords { get; set; }
        public bool IsSuccess { get; set; }
        public string ApiMessage { get; set; }
    }
}
