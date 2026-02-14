using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Data
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<LoginResponse?> Login(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_LoginEmployee", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                await conn.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new LoginResponse
                        {
                            EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Username = reader.GetString(reader.GetOrdinal("Username")),
                            Role = reader.GetString(reader.GetOrdinal("Role")),
                            Status = reader.GetString(reader.GetOrdinal("Status")),
                            ProfileImage = reader.IsDBNull(reader.GetOrdinal("ProfileImage"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("ProfileImage"))
                        };
                    }
                }
            }
            return null;
        }


        public async Task<int> RegisterEmployee(Employee employee)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_RegisterEmployee", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Name", employee.Name);
                cmd.Parameters.AddWithValue("@Designation", employee.Designation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Department", employee.Department ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@JoiningDate", employee.JoiningDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Skillset", employee.Skillset ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Username", employee.Username);
                cmd.Parameters.AddWithValue("@Password", employee.Password);
                cmd.Parameters.AddWithValue("@CreatedBy", employee.CreatedBy ?? "Self");

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }

        public async Task<Employee?> GetEmployeeById(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetEmployeeById", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmployeeID", id);

                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapEmployeeFromReader(reader);
                    }
                }
            }
            return null;
        }

        public async Task<List<Employee>> GetAllEmployees()
        {
            var employees = new List<Employee>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_GetAllEmployees", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                await conn.OpenAsync();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        employees.Add(MapEmployeeFromReader(reader));
                    }
                }
            }
            return employees;
        }

        public async Task<bool> UpdateEmployee(Employee employee)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_UpdateEmployee", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                cmd.Parameters.AddWithValue("@Name", employee.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Designation", employee.Designation ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Department", employee.Department ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@JoiningDate", employee.JoiningDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Skillset", employee.Skillset ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", employee.Status ?? "Active");
                cmd.Parameters.AddWithValue("@ModifiedBy", employee.ModifiedBy ?? "System");

                await conn.OpenAsync();

                var result = await cmd.ExecuteScalarAsync();   // ✅ IMPORTANT
                return Convert.ToInt32(result) > 0;
            }
        }

        public async Task UpdateProfileImage(int id, string imagePath)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("UPDATE Employees SET ProfileImage = @ProfileImage WHERE EmployeeID = @EmployeeID", conn))
            {
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.AddWithValue("@ProfileImage", imagePath ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EmployeeID", id);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }



        private Employee MapEmployeeFromReader(SqlDataReader reader)
        {
            var emp = new Employee();

            int ord;

            ord = reader.GetOrdinal("EmployeeID");
            emp.EmployeeID = reader.IsDBNull(ord) ? 0 : reader.GetInt32(ord);

            ord = reader.GetOrdinal("Name");
            emp.Name = reader.IsDBNull(ord) ? string.Empty : reader.GetString(ord);

            ord = reader.GetOrdinal("Designation");
            emp.Designation = reader.IsDBNull(ord) ? null : reader.GetString(ord);

            ord = reader.GetOrdinal("Address");
            emp.Address = reader.IsDBNull(ord) ? null : reader.GetString(ord);

            ord = reader.GetOrdinal("Department");
            emp.Department = reader.IsDBNull(ord) ? null : reader.GetString(ord);

            ord = reader.GetOrdinal("JoiningDate");
            emp.JoiningDate = reader.IsDBNull(ord) ? null : reader.GetDateTime(ord);

            ord = reader.GetOrdinal("Skillset");
            emp.Skillset = reader.IsDBNull(ord) ? null : reader.GetString(ord);

            ord = reader.GetOrdinal("Username");
            emp.Username = reader.IsDBNull(ord) ? string.Empty : reader.GetString(ord);

            ord = reader.GetOrdinal("Password");
            emp.Password = reader.IsDBNull(ord) ? string.Empty : reader.GetString(ord);

            ord = reader.GetOrdinal("Status");
            emp.Status = reader.IsDBNull(ord) ? string.Empty : reader.GetString(ord);

            ord = reader.GetOrdinal("Role");
            emp.Role = reader.IsDBNull(ord) ? string.Empty : reader.GetString(ord);

            ord = reader.GetOrdinal("CreatedBy");
            emp.CreatedBy = reader.IsDBNull(ord) ? null : reader.GetString(ord);

            ord = reader.GetOrdinal("ModifiedBy");
            emp.ModifiedBy = reader.IsDBNull(ord) ? null : reader.GetString(ord);

            ord = reader.GetOrdinal("CreatedAt");
            emp.CreatedAt = reader.IsDBNull(ord) ? null : reader.GetDateTime(ord);

            ord = reader.GetOrdinal("ModifiedAt");
            emp.ModifiedAt = reader.IsDBNull(ord) ? null : reader.GetDateTime(ord);

            emp.ProfileImage = reader.IsDBNull(reader.GetOrdinal("ProfileImage"))
    ? null
    : reader.GetString(reader.GetOrdinal("ProfileImage"));


            return emp;
        }
    }
}
