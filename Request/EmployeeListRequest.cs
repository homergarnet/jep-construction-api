namespace jep_construction_api.Request
{
    public class EmployeeListRequest
    {
    }

    public class CreateUpdateEmployeeRequest
    {
        public long? Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string Department { get; set; }
        public int HourlyRate { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyRelationship { get; set; }
        public string EmergencyContactNo { get; set; }
        public string UserType { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
