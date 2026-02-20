using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/ef/Employee")] // Explicit route ensures it stays exactly /api/ef/Employee
    public class EmployeeControllerEf : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenService _jwtTokenService;

        public EmployeeControllerEf(AppDbContext context, JwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. Fetch the raw Employee database entity
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Username == request.Username && e.Password == request.Password);

            if (employee == null)
                return Unauthorized(new { message = "Invalid username or password." });

            if (employee.Status != null && employee.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { message = "Account inactive." });

            // 2. Map the Database Entity to the LoginResponse DTO
            var loginResponse = new LoginResponse
            {
                EmployeeID = employee.EmployeeID,
                Name = employee.Name,
                Username = employee.Username,
                Role = employee.Role ?? "User", // Fallback if Role is null in the database
                Status = employee.Status,
                ProfileImage = employee.ProfileImage
            };

            // 3. Generate the token using the strictly typed DTO
            var token = _jwtTokenService.GenerateToken(loginResponse);

            // 4. Return the DTO to the client to avoid leaking the password hash and other metadata
            return Ok(new
            {
                token = token,
                user = loginResponse
            });
        }
        // 🔹 REGISTER
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Employee employee)
        {
            employee.Status = "Active";

            _context.Employees.Add(employee);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    employeeId = employee.EmployeeID,
                    message = "Registration successful"
                });
            }
            catch (DbUpdateException)
            {
                // Catches UNIQUE constraint violations (like duplicate usernames) gracefully
                return BadRequest(new { message = "Database error: Username may already exist." });
            }
        }

        // 🔹 GET BY ID
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        // 🔹 GET ALL
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            // AsNoTracking is perfect for read-only lists to minimize memory overhead
            var employees = await _context.Employees.AsNoTracking().ToListAsync();
            return Ok(employees);
        }

        // 🔹 UPDATE
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
        {
            var existingEmployee = await _context.Employees.FindAsync(id);

            if (existingEmployee == null)
                return NotFound(new { message = "Employee not found." });

            // Safely maps updated values from the request to the tracked entity
            // without overriding fields like primary keys accidentally.
            _context.Entry(existingEmployee).CurrentValues.SetValues(employee);

            // Ensure the ID isn't modified by the payload
            existingEmployee.EmployeeID = id;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Employee updated successfully" });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "An error occurred while updating the database." });
            }
        }

        // 🔹 TOGGLE STATUS (ADMIN ONLY)
        [Authorize(Roles = "Admin")]
        [HttpPatch("ToggleStatus/{id}")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest("Status cannot be empty.");

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
                return NotFound(new { message = "Employee not found." });

            employee.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully" });
        }
    }
}