using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;


namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeRepository _repository;
        private readonly JwtTokenService _jwtTokenService;

        public EmployeeController(EmployeeRepository repository, JwtTokenService jwtTokenService)
        {
            _repository = repository;
            _jwtTokenService = jwtTokenService;
        }


        // 🔹 LOGIN
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var result = await _repository.Login(request.Username, request.Password);

            // ❌ If username or password is wrong
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // ❌ If account is inactive
            if (result.Status != null && result.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Your account is inactive. Please contact admin." });
            }
            var token = _jwtTokenService.GenerateToken(result);

            return Ok(new
            {
                token = token,
                user = result
            });

        }

        // 🔹 REGISTER
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Employee employee)
        {
            try
            {
                employee.Status = "Active"; // Default status

                var employeeId = await _repository.RegisterEmployee(employee);

                return Ok(new { employeeId, message = "Registration successful" });
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // UNIQUE constraint violation
                {
                    return BadRequest(new { message = "Username already exists." });
                }

                return StatusCode(500, new { message = "Database error." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }



        // 🔹 GET EMPLOYEE BY ID
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _repository.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        // 🔹 GET ALL EMPLOYEES
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _repository.GetAllEmployees();
            return Ok(employees);
        }

        // 🔹 UPDATE EMPLOYEE (INCLUDING STATUS)
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(
    int id,
    [FromBody] UpdateEmployeeRequest request,
    [FromQuery] string? modifiedBy)
        {
            try
            {
                var employee = new Employee
                {
                    EmployeeID = id,
                    Name = request.Name,
                    Designation = request.Designation,
                    Address = request.Address,
                    Department = request.Department,
                    JoiningDate = request.JoiningDate,
                    Skillset = request.Skillset,
                    Status = string.IsNullOrEmpty(request.Status) ? "Active" : request.Status,
                    ModifiedBy = string.IsNullOrWhiteSpace(modifiedBy) ? "System" : modifiedBy
                };

                var success = await _repository.UpdateEmployee(employee);

                if (!success)
                {
                    return BadRequest(new { message = "Update failed." });
                }

                return Ok(new { message = "Employee updated successfully" });
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    return BadRequest(new { message = "Username already exists." });
                }

                return StatusCode(500, new { message = "Database error." });
            }
        }


        [HttpPost("upload-image/{id}")]
       
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            byte[] imageBytes;

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            await _repository.UpdateProfileImage(id, imageBytes);

            return Ok("Image saved to database");
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest("Status cannot be empty.");
            }

            var success = await _repository.ToggleEmployeeStatus(id, status);
            if (success)
            {
                return Ok(new { Message = $"Employee status updated to {status} successfully." });
            }

            return NotFound("Employee not found.");
        }

        private bool IsAdmin()
        {
            if (Request.Headers.TryGetValue("X-User-Role", out var vals))
            {
                var role = vals.ToString();
                return role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
