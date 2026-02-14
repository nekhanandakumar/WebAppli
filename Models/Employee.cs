using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoiningDate { get; set; }

        [MaxLength(500)]
        public string? Skillset { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Username { get; set; }

        [Required]
        [MinLength(6)]
        public string? Password { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        [MaxLength(50)]
        public string? Role { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        [MaxLength(255)]
        public string? ProfileImage { get; set; }

    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string? ProfileImage { get; set; }
    }

}
