namespace jep_construction_api.DTOS
{
    public class AssignProjectDto
    {
        public long Id { get; set; }
        public string? Email { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? EmployeeName { get; set; }
        public string? MobileNumber { get; set; }
        public string? ProfileImage { get; set; }
        public string? Position { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }
    }
}
