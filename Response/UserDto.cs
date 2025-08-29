using System.ComponentModel.DataAnnotations;

namespace barangay_crime_compliant_api.DTOS
{

    public class UserDto
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string Phone { get; set; } = null!;
        public string? HouseNo { get; set; }
        public string? Street { get; set; }
        public string? Village { get; set; }
        public string? UnitFloor { get; set; }
        public string? Building { get; set; }
        public string? ProvinceCode { get; set; }
        public string? CityCode { get; set; }
        public string? BrgyCode { get; set; }
        public string? ZipCode { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string? UserType { get; set; }
        public string? Token { get; set; }
        public string? ValidIdImage { get; set; }
        public string? SelfieIdImage { get; set; }
        public string? ProvinceName { get; set; }
        public string? CityName { get; set; }
        public string? BarangayName { get; set; }
        public string BirthDateStr { get; set; }
        public long id { get; set; }
        public string? name { get; set; }
    }

    public class LoginDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? UserType { get; set; }

    }

    public class UpdatePasswordDto
    {

        [Required]
        public string NewPassword { get; set; } = "";
        [Required]
        public string Email { get; set; } = "";
        [Required]
        public string Token { get; set; } = "";

    }

    public class PoliceInOutDto
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string Name { get;set; }
        public string? Type { get; set; }
        public string? DateTimeCreated { get; set; }
        public string? DateTimeUpdated { get; set; }
    }



}