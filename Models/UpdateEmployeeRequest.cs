
using System;

namespace EmployeeManagementAPI.Models
{
    public class UpdateEmployeeRequest
    {
        public string Name { get; set; }
        public string? Designation { get; set; }
        public string? Address { get; set; }
        public string? Department { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string? Skillset { get; set; }
        public string Status { get; set; }
    }
}
