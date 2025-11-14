namespace jep_construction_api.DTOS
{
    public class AttendanceDto
    {
        public long Id { get; set; }
        public string EmployeeNumber { get; set; }
        public string EmployeeName { get; set; }
        public string Location { get; set; }
        public DateTime TimeIn { get; set; }
        public string TimeInImage { get; set; }
        public DateTime TimeOut { get; set; }
        public string TimeOutImage { get; set; }
        public string Duration { get; set; }
        //public DateTime DateTimeCreated { get; set; }
    }
}
