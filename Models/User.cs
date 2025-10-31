using System;
using System.Collections.Generic;

namespace jep_construction_api.Models
{
    public partial class User
    {
        public User()
        {
            AssignProjects = new HashSet<AssignProject>();
            EmployeeAttendances = new HashSet<EmployeeAttendance>();
            EmployeePayslips = new HashSet<EmployeePayslip>();
            Inventories = new HashSet<Inventory>();
            MessageSenders = new HashSet<Message>();
            MessageUsers = new HashSet<Message>();
            ProjectManagements = new HashSet<ProjectManagement>();
            Reviews = new HashSet<Review>();
        }

        public long Id { get; set; }
        public string? Email { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? MobileNumber { get; set; }
        public string? Position { get; set; }
        public decimal? Salary { get; set; }
        public string? Status { get; set; }
        public string? Password { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? UserType { get; set; }
        public string? ProfileImage { get; set; }
        public bool? IsEnabled { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeUpdated { get; set; }

        public virtual ICollection<AssignProject> AssignProjects { get; set; }
        public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; }
        public virtual ICollection<EmployeePayslip> EmployeePayslips { get; set; }
        public virtual ICollection<Inventory> Inventories { get; set; }
        public virtual ICollection<Message> MessageSenders { get; set; }
        public virtual ICollection<Message> MessageUsers { get; set; }
        public virtual ICollection<ProjectManagement> ProjectManagements { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
