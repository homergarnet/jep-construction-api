namespace jep_construction_api.Request
{
    public class CreateUpdateProfileRequest
    {

        public long? Id { get; set; }
        public IFormFile ProfileImage { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Position { get; set; }

    }

}
