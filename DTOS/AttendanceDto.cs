namespace jep_construction_api.DTOS
{
    public class AttendanceDto
    {
        public long Id { get; set; }
        public string EmployeeNumber { get; set; }
        public string ClientName { get; set; }
        public string Location { get; set; }
        public DateTime TimeInOut { get; set; }
        public string TimeInOutType { get; set; }
        public string TimeInOutImage { get; set; } // base64 string
        public DateTime DateTimeCreated { get; set; }
    }
}
