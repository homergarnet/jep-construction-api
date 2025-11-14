using Dapper;
using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Models;
using jep_construction_api.Request;
using jep_construction_api.Response;
using Microsoft.Data.SqlClient;
using System.Data;

namespace jep_construction_api.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;
        private string attendanceQuery = string.Empty;

        public AttendanceService(IConfiguration configuration, Jep_ConstructionContext db, IWebHostEnvironment env)
        {
            this.configuration = configuration;
            this.db = db;
            _env = env;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
        }

        public async Task<AttendanceResponse> CreateTimeInOut(CreateTimeInOutRequest req)
        {

            var response = new AttendanceResponse
            {
                AttendanceList = new List<AttendanceDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            if (string.IsNullOrEmpty(req.TimeInOutImage))
                throw new ArgumentException("No image provided.");

            // Convert base64 → bytes
            var base64Data = req.TimeInOutImage.Contains(",")
                ? req.TimeInOutImage.Split(',')[1]
                : req.TimeInOutImage;

            var bytes = Convert.FromBase64String(base64Data);

            // Save file
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"attendance_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var filePath = Path.Combine(uploadsFolder, fileName);
            await File.WriteAllBytesAsync(filePath, bytes);

            //// check if there is a time in already for this user and for current day
            //if (db.EmployeeAttendances.Any(z => z.EmployeeId == req.EmployeeId
            //&& z.DateTimeCreated.Date == Common.DateTimeNow("Singapore Standard Time").Date))
            //{

            //}

            // Dapper insert
            using (IDbConnection conn = new SqlConnection(_connectionString))
            {
                attendanceQuery = @"
                    INSERT INTO [dbo].[EmployeeAttendance] (EmployeeId, Location, TimeInOut, TimeInOutType, TimeInOutImage, 
                    DateTimeCreated)
                    VALUES (@EmployeeId, @Location, @DateTimeCreated, @TimeInOutType, @TimeInOutImage, @DateTimeCreated);";

                var rowsAffected = await conn.ExecuteAsync(attendanceQuery, new
                {
                    EmployeeId = req.EmployeeId,
                    Location = req.Location,
                    TimeInOut = Common.DateTimeNow("Singapore Standard Time"),
                    TimeInOutType = req.TimeInOutType,
                    TimeInOutImage = $"uploads\\{fileName}",
                    DateTimeCreated = Common.DateTimeNow("Singapore Standard Time")
                });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = AttendanceConstants.CREATE_ATTENDANCE_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = AttendanceConstants.CREATE_ATTENDANCE_FAILED;
                }

                //return $"/uploads/{fileName}";
                return response;

            }



        }

        public AttendanceResponse GetAttendanceById(long id)
        {
            var response = new AttendanceResponse
            {
                AttendanceList = new List<AttendanceDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };


            if (id <= 0)
            {
                response.ApiMessage = AttendanceConstants.INVALID_ATTENDANCE_ID;
                return response;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                    WITH AttendancePairs AS (
                        SELECT
                            tin.Id AS TimeInId,
                            tin.EmployeeId,
                            tin.Location,
                            tin.TimeInOut AS TimeIn,
                            tin.TimeInOutImage AS TimeInImage,
                            tout.TimeInOut AS TimeOut,
                            tout.TimeInOutImage AS TimeOutImage,
                            DATEDIFF(MINUTE, tin.TimeInOut, tout.TimeInOut) AS DurationMinutes
                        FROM EmployeeAttendance tin
                        OUTER APPLY (
                            SELECT TOP 1 *
                            FROM EmployeeAttendance t
                            WHERE 
                                t.EmployeeId = tin.EmployeeId
                                AND t.TimeInOutType = 'out'
                                AND t.TimeInOut > tin.TimeInOut
                            ORDER BY t.TimeInOut
                        ) tout
                        WHERE tin.TimeInOutType = 'in'
                          AND tin.IsEnabled = 1
                    )
                    SELECT 
                        ap.TimeInId AS Id,
                        u.EmployeeNumber,
                        (u.Firstname + ' ' + u.Lastname) AS EmployeeName,
                        ap.Location,

                        FORMAT(ap.TimeIn, 'MM/dd/yyyy hh:mm tt') AS TimeIn,
                        ap.TimeInImage,

                        FORMAT(ap.TimeOut, 'MM/dd/yyyy hh:mm tt') AS TimeOut,
                        ap.TimeOutImage,

                        CONCAT(ap.DurationMinutes / 60, 'H') AS Duration
                    FROM AttendancePairs ap
                    INNER JOIN [User] u ON u.Id = ap.EmployeeId
                    WHERE ap.TimeInId = @Id;
                ";


                var attendance = connection.QueryFirstOrDefault<AttendanceDto>(query, new { Id = id });

                if (attendance != null)
                {
                    response.AttendanceList.Add(attendance);
                    response.TotalRecords = 1;
                    response.IsSuccess = true;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = AttendanceConstants.ATTENDANCE_NOT_FOUND;
                }
            }

            return response;
        }

        public AttendanceResponse GetAttendanceList(string keyword, long? userId, int page, int pageSize)
        {

            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Equals("not/a"))
            {
                // no keyword filter → return all employees (paged)
                keyword = "";
            }
            var response = new AttendanceResponse
            {
                AttendanceList = new List<AttendanceDto>(), // or UserList depending on your model
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };


            keyword = keyword ?? string.Empty;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var dataQuery = "";
                // if not admin
                if (userId != 0)
                {
                    // Total Count
                    var countQuery = @"
                        WITH AttendancePairs AS (
                            SELECT
                                tin.Id AS TimeInId,
                                tin.EmployeeId
                            FROM EmployeeAttendance tin
                            OUTER APPLY (
                                SELECT TOP 1 *
                                FROM EmployeeAttendance t
                                WHERE 
                                    t.EmployeeId = tin.EmployeeId
                                    AND t.TimeInOutType = 'out'
                                    AND t.TimeInOut > tin.TimeInOut
                                ORDER BY t.TimeInOut
                            ) tout
                            WHERE tin.TimeInOutType = 'in'
                              AND tin.IsEnabled = 1
                        )
                        SELECT COUNT(*)
                        FROM AttendancePairs ap
                        WHERE ap.EmployeeId = @UserId
                    ";


                    var totalCount = connection.ExecuteScalar<long>(countQuery, new
                    {
                        Keyword = keyword,
                        UserId = userId
                    });

                    // Paginated Data
                    dataQuery = @"
                        WITH AttendancePairs AS (
                            SELECT
                                tin.Id AS TimeInId,
                                tin.EmployeeId,
                                tin.Location,
                                tin.TimeInOut AS TimeIn,
                                tin.TimeInOutImage AS TimeInImage,
                                tout.TimeInOut AS TimeOut,
                                tout.TimeInOutImage AS TimeOutImage,
                                DATEDIFF(MINUTE, tin.TimeInOut, tout.TimeInOut) AS DurationMinutes
                            FROM EmployeeAttendance tin
                            OUTER APPLY (
                                SELECT TOP 1 *
                                FROM EmployeeAttendance t
                                WHERE 
                                    t.EmployeeId = tin.EmployeeId
                                    AND t.TimeInOutType = 'out'
                                    AND t.TimeInOut > tin.TimeInOut
                                ORDER BY t.TimeInOut
                            ) tout
                            WHERE tin.TimeInOutType = 'in'
                              AND tin.IsEnabled = 1
                        )
                        SELECT 
                            ap.TimeInId AS Id,
                            u.EmployeeNumber,
                            (u.Firstname + ' ' + u.Lastname) AS EmployeeName,
                            ap.Location,

                            FORMAT(ap.TimeIn, 'MM/dd/yyyy hh:mm tt') AS TimeIn,
                            ap.TimeInImage,

                            FORMAT(ap.TimeOut, 'MM/dd/yyyy hh:mm tt') AS TimeOut,
                            ap.TimeOutImage,

                            CONCAT(ap.DurationMinutes / 60, 'H') AS Duration
                        FROM AttendancePairs ap
                        INNER JOIN [User] u ON u.Id = ap.EmployeeId
                        WHERE 
                            ap.EmployeeId = @UserId
                        ORDER BY ap.TimeInId DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY
                    ";

                    var data = connection.Query<AttendanceDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                        UserId = userId,
                    }).ToList();

                    // Set response
                    response.AttendanceList = data;
                    response.TotalRecords = totalCount;
                }
                // admin
                else
                {
                    // Total Count
                    var countQuery = @"
                        WITH AttendancePairs AS (
                            SELECT
                                tin.Id AS TimeInId,
                                tin.EmployeeId
                            FROM EmployeeAttendance tin
                            OUTER APPLY (
                                SELECT TOP 1 *
                                FROM EmployeeAttendance t
                                WHERE 
                                    t.EmployeeId = tin.EmployeeId
                                    AND t.TimeInOutType = 'out'
                                    AND t.TimeInOut > tin.TimeInOut
                                ORDER BY t.TimeInOut
                            ) tout
                            WHERE tin.TimeInOutType = 'in'
                              AND tin.IsEnabled = 1
                        )
                        SELECT COUNT(*)
                        FROM AttendancePairs ap
                    ";


                    var totalCount = connection.ExecuteScalar<long>(countQuery, new
                    {
                        Keyword = keyword,
                    });

                    // Paginated Data
                    dataQuery = @"
                        WITH AttendancePairs AS (
                            SELECT
                                tin.Id AS TimeInId,
                                tin.EmployeeId,
                                tin.Location,
                                tin.TimeInOut AS TimeIn,
                                tin.TimeInOutImage AS TimeInImage,
                                tout.TimeInOut AS TimeOut,
                                tout.TimeInOutImage AS TimeOutImage,
                                DATEDIFF(MINUTE, tin.TimeInOut, tout.TimeInOut) AS DurationMinutes
                            FROM EmployeeAttendance tin
                            OUTER APPLY (
                                SELECT TOP 1 *
                                FROM EmployeeAttendance t
                                WHERE 
                                    t.EmployeeId = tin.EmployeeId
                                    AND t.TimeInOutType = 'out'
                                    AND t.TimeInOut > tin.TimeInOut
                                ORDER BY t.TimeInOut
                            ) tout
                            WHERE tin.TimeInOutType = 'in'
                              AND tin.IsEnabled = 1
                        )
                        SELECT 
                            ap.TimeInId AS Id,
                            u.EmployeeNumber,
                            (u.Firstname + ' ' + u.Lastname) AS EmployeeName,
                            ap.Location,

                            FORMAT(ap.TimeIn, 'MM/dd/yyyy hh:mm tt') AS TimeIn,
                            ap.TimeInImage,

                            FORMAT(ap.TimeOut, 'MM/dd/yyyy hh:mm tt') AS TimeOut,
                            ap.TimeOutImage,

                            CONCAT(ap.DurationMinutes / 60, 'H') AS Duration
                        FROM AttendancePairs ap
                        INNER JOIN [User] u ON u.Id = ap.EmployeeId
                        ORDER BY ap.TimeInId DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY
                    ";

                    var data = connection.Query<AttendanceDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                    }).ToList();

                    // Set response
                    response.AttendanceList = data;
                    response.TotalRecords = totalCount;
                }

                response.IsSuccess = true;

            }

            return response;
        }

        public AttendanceResponse SoftDeleteAttendanceById(string id)
        {
            var response = new AttendanceResponse
            {
                AttendanceList = new List<AttendanceDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[EmployeeAttendance] 
                            SET IsEnabled = 0
                            WHERE Id = @Id";
                int rowsAffected = connection.Execute(sql, new { Id = id });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = AttendanceConstants.SOFT_DELETE_ATTENDANCE_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = AttendanceConstants.SOFT_DELETE_ATTENDANCE_FAILED;
                }

                return response;

            }
        }

        public AttendanceResponse UpdateAttendance(UpdateAttendanceRequest req)
        {
            var response = new AttendanceResponse
            {
                AttendanceList = new List<AttendanceDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using var connection = new SqlConnection(_connectionString);
            var dateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
            attendanceQuery = @"UPDATE [dbo].[EmployeeAttendance] 
                            SET TimeInOut = @TimeInOut, TimeInOutType = @TimeInOutType, DateTimeUpdated = @DateTimeUpdated
                            WHERE Id = @Id
                            ";

            var rowsInserted = connection.Execute(attendanceQuery, new
            {
                Id = req.Id,
                TimeInOut = req.TimeInOut,
                TimeInOutType = req.TimeInOutType,
                DateTimeUpdated = dateTimeNow,
            });

            if (rowsInserted > 0)
            {

                response.IsSuccess = true;
                response.ApiMessage = AttendanceConstants.UPDATE_ATTENDANCE_SUCCESS;

            }
            else
            {
                response.IsSuccess = false;
                response.ApiMessage = AttendanceConstants.UPDATE_ATTENDANCE_FAILED;
            }

            return response;
        }
    }
}
