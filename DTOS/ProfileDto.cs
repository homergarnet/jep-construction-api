namespace jep_construction_api.DTOS
{
    public class ProfileDto
    {
        public long Id { get; set; }
        public string? ProfileImage { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Position { get; set; }
    }
}
