namespace jep_construction_api.Request
{
    public class AttendanceRequest
    {
    }

    public class CreateTimeInOutRequest
    {
        public long EmployeeId { get; set; }
        public string Location { get; set; }
        public string TimeInOutType { get; set; }
        public string TimeInOutImage { get; set; } // base64 string
    }

    public class UpdateAttendanceRequest
    {
        public long Id { get; set; }
        public DateTime TimeInOut { get; set; }
        public string TimeInOutType { get; set; }
    }
}
