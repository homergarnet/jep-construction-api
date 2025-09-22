using Dapper;
using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Models;
using jep_construction_api.Request;
using jep_construction_api.Response;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace jep_construction_api.Services
{
    public class EmployeeListService : IEmployeeListService
    {

        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private string employeeQuery = string.Empty;
        public EmployeeListService(IConfiguration configuration, Jep_ConstructionContext db)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
        }

        public EmployeeListResponse CreateEmployee(CreateUpdateEmployeeRequest createEmployeeReq)
        {
            var response = new EmployeeListResponse
            {
                UserList = new List<UserDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };


            // user.Id = item.Id;
            var userExist = db.Users.Any(z => z.Email == createEmployeeReq.Email);
            if (userExist)
            {
                response.IsSuccess = false;
                response.ApiMessage = EmployeeListConstants.EMAIL_ALREADY_EXIST;

            }
            else
            {
                User user = new User();
                var email = createEmployeeReq.Email?.Trim() ?? "";
                if (!string.IsNullOrEmpty(email) && new EmailAddressAttribute().IsValid(email))
                {
                    user.Email = email;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = AuthConstants.INVALID_EMAIL_ADDRESS;
                    return response;
                    //throw new ArgumentException("Invalid email address.");
                }

                var lastId = db.Users.OrderByDescending(u => u.Id)
                     .Select(u => u.Id)
                     .FirstOrDefault();
                lastId++;

                user.EmployeeNumber = Common.GenEmployeeNumber(lastId.ToString());
                user.Firstname = createEmployeeReq.FirstName?.Trim() ?? "";
                user.Lastname = createEmployeeReq.LastName?.Trim() ?? "";
                user.MobileNumber = createEmployeeReq.MobileNumber?.Trim() ?? "";
                user.Position = createEmployeeReq.Position?.Trim() ?? "";
                user.Salary = createEmployeeReq.Salary;
                user.Status = createEmployeeReq.Status?.Trim() ?? "";
                user.Password = BCrypt.Net.BCrypt.HashPassword(configuration["DefaultPassword:Password"]);
                user.Address = createEmployeeReq.Address?.Trim() ?? "";
                user.DateOfBirth = createEmployeeReq.DateOfBirth;
                user.UserType = "employee";
                user.DateTimeCreated = Common.DateTimeNow("Singapore Standard Time");
                db.Users.Add(user);
                db.SaveChanges();
                response.IsSuccess = true;
                response.ApiMessage = EmployeeListConstants.CREATE_EMPLOYEE_ACCOUNT_SUCCESS;

            }

            return response;
        }

        public EmployeeListResponse GetEmployeeById(long id)
        {
            var response = new EmployeeListResponse
            {
                UserList = new List<UserDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            try
            {
                if (id <= 0)
                {
                    response.ApiMessage = EmployeeListConstants.INVALID_EMPLOYEE_ID;
                    return response;
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"SELECT * FROM [dbo].[User] WHERE Id = @Id";

                    var employee = connection.QueryFirstOrDefault<UserDto>(query, new { Id = id });

                    if (employee != null)
                    {
                        response.UserList.Add(employee);
                        response.TotalRecords = 1;
                        response.IsSuccess = true;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.ApiMessage = EmployeeListConstants.EMPLOYEE_NOT_FOUND;
                    }
                }
            }
            catch (Exception ex)
            {
                response.ApiMessage = ex.Message;
            }

            return response;
        }

        public EmployeeListResponse GetEmployeeList(string keyword, int page, int pageSize)
        {
            var response = new EmployeeListResponse
            {
                UserList = new List<UserDto>(), // or UserList depending on your model
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };


            keyword = keyword ?? string.Empty;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Total Count
                var countQuery = @"
                        SELECT COUNT(*)
                        FROM [dbo].[User]
                        WHERE (@Keyword = '' OR Email LIKE '%' + @Keyword + '%')";

                var totalCount = connection.ExecuteScalar<long>(countQuery, new { Keyword = keyword });

                // Paginated Data
                var dataQuery = @"
                SELECT *
                FROM [dbo].[User]
                WHERE (@Keyword = '' OR Email LIKE '%' + @Keyword + '%')
                ORDER BY Id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

                var data = connection.Query<UserDto>(dataQuery, new
                {
                    Keyword = keyword,
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize
                }).ToList();

                // Set response
                response.UserList = data;
                response.TotalRecords = totalCount;
                response.IsSuccess = true;
            }

            return response;
        }

        public EmployeeListResponse SoftDeleteEmployeeById(string id)
        {
            var response = new EmployeeListResponse
            {
                UserList = new List<UserDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[User] 
                            SET IsEnabled = 0
                            WHERE Id = @Id";
                int rowsAffected = connection.Execute(sql, new { Id = id });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = EmployeeListConstants.SOFT_DELETE_EMPLOYEE_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = EmployeeListConstants.SOFT_DELETE_EMPLOYEE_FAILED;
                }

                return response;

            }
        }

        public EmployeeListResponse UpdateEmployee(CreateUpdateEmployeeRequest updateEmployeeReq)
        {
            var response = new EmployeeListResponse
            {
                UserList = new List<UserDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using var connection = new SqlConnection(_connectionString);
            var dateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
            employeeQuery = @"UPDATE [dbo].[User] 
                            SET Email = @Email, FirstName = @FirstName, LastName = @LastName, MobileNumber = @MobileNumber,
                            Position = @Position, Salary = @Salary, Status = @Status, Address = @Address,
                            DateOfBirth = @DateOfBirth, DateTimeUpdated = @DateTimeUpdated
                            WHERE Id = @Id
                            ";

            var rowsInserted = connection.Execute(employeeQuery, new
            {
                Id = updateEmployeeReq.Id,
                Email = updateEmployeeReq.Email,
                FirstName = updateEmployeeReq.FirstName,
                LastName = updateEmployeeReq.LastName,
                MobileNumber = updateEmployeeReq.MobileNumber,
                Position = updateEmployeeReq.Position,
                Salary = updateEmployeeReq.Salary,
                Status = updateEmployeeReq.Status,
                Address = updateEmployeeReq.Address,
                DateOfBirth = updateEmployeeReq.DateOfBirth,
                DateTimeUpdated = dateTimeNow,
            });

            if (rowsInserted > 0)
            {

                response.IsSuccess = true;
                response.ApiMessage = EmployeeListConstants.UPDATE_EMPLOYEE_SUCCESS;

            }
            else
            {
                response.IsSuccess = false;
                response.ApiMessage = EmployeeListConstants.UPDATE_EMPLOYEE_FAILED;
            }

            return response;

        }
    }
}
