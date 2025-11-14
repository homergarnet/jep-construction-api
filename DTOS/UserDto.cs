using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace jep_construction_api.DTOS
{

    public class UserDto
    {
        public long Id { get; set; }
        public string? Email { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? MobileNumber { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public string? Status { get; set; }
        [JsonIgnore] // will not appear in API responses
        public string? Password { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? Department { get; set; }
        public int? HourlyRate { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyRelationship { get; set; }
        public string? EmergencyContactNo { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? UserType { get; set; }
        public string? ProfileImage { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }
    }

    public class LoginDto
    {
        public string? Email { get; set; }
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