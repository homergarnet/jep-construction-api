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
        public string AccountType { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
