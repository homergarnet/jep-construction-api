using Dapper;
using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Models;
using jep_construction_api.Request;
using jep_construction_api.Response;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace jep_construction_api.Services
{
    public class AssignProjectService : IAssignProjectService
    {

        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private string assignProjectQuery = string.Empty;

        public AssignProjectService(IConfiguration configuration, Jep_ConstructionContext db)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
        }

        public AssignProjectResponse CreateAssignProject(AssignProjectCreateUpdateRequest req)
        {
            var dateTimeNow = Common.DateTimeNow("Singapore Standard Time");
            var response = new AssignProjectResponse
            {
                AssignProjectList = new List<AssignProjectDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            var assignProjectExist = db.AssignProjects.Any(z => z.UserId == req.UserId && z.ProjectId == req.ProjectId && req.StartDate.Date <= z.EndDate.Date);
            if (assignProjectExist)
            {

                response.IsSuccess = false;
                response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_NOT_YET_DONE;

            }
            else
            {

                AssignProject assignProject = new AssignProject();
                assignProject.UserId = req.UserId;
                assignProject.ProjectId = req.ProjectId;
                assignProject.StartDate = req.StartDate;
                assignProject.EndDate = req.EndDate;
                assignProject.DateTimeCreated = dateTimeNow;
                db.AssignProjects.Add(assignProject);
                db.SaveChanges();
                response.IsSuccess = true;
                response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_CREATE_SUCCESS;

            }

            return response;
        }

        public AssignProjectResponse GetAssignProjectById(long id)
        {
            var response = new AssignProjectResponse
            {
                AssignProjectList = new List<AssignProjectDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };


            if (id <= 0)
            {
                response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_INVALID_ID;
                return response;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var countQuery = @"SELECT COUNT(*) 
                    FROM [dbo].[AssignProject] ap
                    INNER JOIN [dbo].[User] u ON u.Id = ap.UserId
                    INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = ap.ProjectId
                    WHERE ap.IsEnabled = 1 AND ap.Id = @Id";

                var query = @"
                SELECT 
                    ap.Id, 
                    ap.UserId, 
                    pm.Id AS ProjectId,
                    (COALESCE(uClient.Firstname, '') + ' ' + COALESCE(uClient.Lastname, '')) AS ClientName,
                    pm.ProjectName, 
                    ap.StartDate, 
                    ap.EndDate, 
                    ap.DateTimeCreated, 
                    ap.DateTimeUpdated, 
                    u.Email, 
                    u.EmployeeNumber, 
                    (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS EmployeeName, 
                    u.MobileNumber, 
                    u.ProfileImage,
                    u.Position, 
                    pm.Location
                FROM [dbo].[AssignProject] ap
                INNER JOIN [dbo].[User] u ON u.Id = ap.UserId
                INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = ap.ProjectId
                LEFT JOIN [dbo].[User] uClient ON uClient.Id = pm.UserId
                WHERE ap.IsEnabled = 1 AND ap.Id = @Id";
                var totalCount = connection.ExecuteScalar<long>(countQuery, new { Id = id });
                var assignProject = connection.QueryFirstOrDefault<AssignProjectDto>(query, new { Id = id });

                if (assignProject != null)
                {
                    response.AssignProjectList.Add(assignProject);
                    response.TotalRecords = totalCount;
                    response.IsSuccess = true;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_NOT_FOUND;
                }
            }

            return response;
        }

        public AssignProjectResponse GetAssignProjectList(string keyword, long? userId, int page, int pageSize)
        {
            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Equals("not/a"))
            {
                // no keyword filter → return all employees (paged)
                keyword = "";
            }
            var response = new AssignProjectResponse
            {
                AssignProjectList = new List<AssignProjectDto>(), // or UserList depending on your model
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
                    var countQuery = @"SELECT COUNT(*) FROM [dbo].[AssignProject] ap
                    INNER JOIN [dbo].[User] u ON u.Id = ap.UserId
                    INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = ap.ProjectId
                    LEFT JOIN [dbo].[User] uClient ON uClient.Id = pm.UserId
                    WHERE (@Keyword = '' OR LOWER(LTRIM(RTRIM(CONCAT(u.Firstname, ' ', u.Lastname)))) LIKE '%' + LOWER(@Keyword) + '%' 
                    OR pm.ProjectName LIKE '%' + LOWER(@Keyword) + '%')
                    AND ap.IsEnabled = 1
                    AND u.Id = @UserId";

                    var totalCount = connection.ExecuteScalar<long>(countQuery, new
                    {
                        Keyword = keyword,
                        UserId = userId
                    });

                    // Paginated Data
                    dataQuery = @"
                    SELECT 
                    ap.Id, 
                    ap.UserId, 
                    pm.Id AS ProjectId,
                    (COALESCE(uClient.Firstname, '') + ' ' + COALESCE(uClient.Lastname, '')) AS ClientName,
                    pm.ProjectName, 
                    ap.StartDate, 
                    ap.EndDate, 
                    ap.DateTimeCreated, 
                    ap.DateTimeUpdated, 
                    u.Email, 
                    u.EmployeeNumber, 
                    (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS EmployeeName, 
                    u.MobileNumber, 
                    u.ProfileImage,
                    u.Position, 
                    pm.Location
                    FROM [dbo].[AssignProject] ap
                    INNER JOIN [dbo].[User] u ON u.Id = ap.UserId
                    INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = ap.ProjectId
                    LEFT JOIN [dbo].[User] uClient ON uClient.Id = pm.UserId
                    WHERE (@Keyword = '' OR LOWER(LTRIM(RTRIM(CONCAT(u.Firstname, ' ', u.Lastname)))) LIKE '%' + LOWER(@Keyword) + '%' 
                    OR pm.ProjectName LIKE '%' + LOWER(@Keyword) + '%')
                    AND ap.IsEnabled = 1
                    AND u.Id = @UserId
                    ORDER BY ap.Id DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                    var data = connection.Query<AssignProjectDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                        UserId = userId,
                    }).ToList();

                    // Set response
                    response.AssignProjectList = data;
                    response.TotalRecords = totalCount;
                }
                // admin
                else
                {
                    // Total Count
                    var countQuery = @"SELECT COUNT(*) FROM [dbo].[AssignProject] ap
                    INNER JOIN [dbo].[User] u ON u.Id = ap.UserId
                    INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = ap.ProjectId
                    LEFT JOIN [dbo].[User] uClient ON uClient.Id = pm.UserId
                    WHERE (@Keyword = '' OR LOWER(LTRIM(RTRIM(CONCAT(u.Firstname, ' ', u.Lastname)))) LIKE '%' + LOWER(@Keyword) + '%' 
                    OR pm.ProjectName LIKE '%' + LOWER(@Keyword) + '%')
                    AND ap.IsEnabled = 1";

                    var totalCount = connection.ExecuteScalar<long>(countQuery, new
                    {
                        Keyword = keyword,
                    });

                    // Paginated Data
                    dataQuery = @"
                    SELECT 
                    ap.Id, 
                    u.Id AS UserId, 
                    pm.Id AS ProjectId,
                    (COALESCE(uClient.Firstname, '') + ' ' + COALESCE(uClient.Lastname, '')) AS ClientName,
                    pm.ProjectName, 
                    ap.StartDate, 
                    ap.EndDate, 
                    ap.DateTimeCreated, 
                    ap.DateTimeUpdated, 
                    u.Email, 
                    u.EmployeeNumber, 
                    (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS EmployeeName, 
                    u.MobileNumber, 
                    u.ProfileImage,
                    u.Position, 
                    pm.Location
                    FROM [dbo].[AssignProject] ap
                    INNER JOIN [dbo].[User] u ON u.Id = ap.UserId
                    INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = ap.ProjectId
                    LEFT JOIN [dbo].[User] uClient ON uClient.Id = pm.UserId
                    WHERE (@Keyword = '' OR LOWER(LTRIM(RTRIM(CONCAT(u.Firstname, ' ', u.Lastname)))) LIKE '%' + LOWER(@Keyword) + '%' 
                    OR pm.ProjectName LIKE '%' + LOWER(@Keyword) + '%')
                    AND ap.IsEnabled = 1
                    ORDER BY ap.Id DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                    var data = connection.Query<AssignProjectDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                    }).ToList();

                    // Set response
                    response.AssignProjectList = data;
                    response.TotalRecords = totalCount;
                }

                response.IsSuccess = true;

            }

            return response;
        }

        public CNamePNameResponse GetCNamePNameList(string keyword, long? userId, int page, int pageSize)
        {

            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Equals("not/a"))
            {
                // no keyword filter → return all employees (paged)
                keyword = "";
            }
            var response = new CNamePNameResponse
            {
                CNamePNameList = new List<CNamePNameDto>(), // or UserList depending on your model
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            keyword = keyword ?? string.Empty;

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var dataQuery = "";
                if (userId != 0)
                {

                    // Total Count
                    var countQuery = @"SELECT COUNT(*) FROM [dbo].[ProjectManagement] pm
                    INNER JOIN [dbo].[User] u ON u.Id = pm.UserId 
                    WHERE (@Keyword = '' OR LOWER(LTRIM(RTRIM(CONCAT(u.Firstname, ' ', u.Lastname)))) LIKE '%' + LOWER(@Keyword) + '%' 
                    OR pm.ProjectName LIKE '%' + LOWER(@Keyword) + '%') AND pm.IsEnabled = 1 AND u.Id = @UserId";

                    var totalCount = connection.ExecuteScalar<long>(countQuery, new
                    {
                        Keyword = keyword,
                        UserId = userId,
                    });

                    // Paginated Data
                    dataQuery = @"SELECT pm.Id, pm.UserId, (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName, pm.ProjectName
                    FROM [dbo].[ProjectManagement] pm
                    INNER JOIN [dbo].[User] u ON u.Id = pm.UserId 
                    WHERE (@Keyword = '' OR LOWER(LTRIM(RTRIM(CONCAT(u.Firstname, ' ', u.Lastname)))) LIKE '%' + LOWER(@Keyword) + '%' 
                    OR pm.ProjectName LIKE '%' + LOWER(@Keyword) + '%') AND pm.IsEnabled = 1 AND u.Id = @UserId
                    ORDER BY pm.Id DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                    var data = connection.Query<CNamePNameDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                        UserId = userId,
                    }).ToList();

                    // Set response
                    response.CNamePNameList = data;
                    response.TotalRecords = totalCount;
                    response.IsSuccess = true;

                }
                else
                {
                    // Total Count
                    var countQuery = @"SELECT COUNT(*) FROM [dbo].[ProjectManagement] pm
                    INNER JOIN [dbo].[User] u ON u.Id = pm.UserId 
                    WHERE (@Keyword = '' OR LOWER(LTRIM(RTRIM(CONCAT(u.Firstname, ' ', u.Lastname)))) LIKE '%' + LOWER(@Keyword) + '%' 
                    OR pm.ProjectName LIKE '%' + LOWER(@Keyword) + '%') AND pm.IsEnabled = 1";

                    var totalCount = connection.ExecuteScalar<long>(countQuery, new
                    {
                        Keyword = keyword,
                    });

                    // Paginated Data
                    dataQuery = @"SELECT pm.Id, pm.UserId, (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName, pm.ProjectName
                    FROM [dbo].[ProjectManagement] pm
                    INNER JOIN [dbo].[User] u ON u.Id = pm.UserId 
                    WHERE (@Keyword = '' OR LOWER(LTRIM(RTRIM(CONCAT(u.Firstname, ' ', u.Lastname)))) LIKE '%' + LOWER(@Keyword) + '%' 
                    OR pm.ProjectName LIKE '%' + LOWER(@Keyword) + '%') AND pm.IsEnabled = 1
                    ORDER BY pm.Id DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                    var data = connection.Query<CNamePNameDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                    }).ToList();

                    // Set response
                    response.CNamePNameList = data;
                    response.TotalRecords = totalCount;
                    response.IsSuccess = true;

                }
            }

            return response;

        }

        public AssignProjectResponse SoftDeleteAssignProjectById(string id)
        {
            var response = new AssignProjectResponse
            {
                AssignProjectList = new List<AssignProjectDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[AssignProject] 
                            SET IsEnabled = 0
                            WHERE Id = @Id";
                int rowsAffected = connection.Execute(sql, new { Id = id });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_SOFT_DELETE_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_SOFT_DELETE_FAILED;
                }

                return response;

            }
        }

        public AssignProjectResponse UpdateAssignProject(AssignProjectCreateUpdateRequest req)
        {

            var response = new AssignProjectResponse
            {
                AssignProjectList = new List<AssignProjectDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };
            var assignProjectExist = db.AssignProjects.Any(z => z.UserId == req.UserId && z.ProjectId == req.ProjectId && req.StartDate.Date <= z.EndDate.Date);
            if (assignProjectExist)
            {

                response.IsSuccess = false;
                response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_NOT_YET_DONE;

            }
            else
            {
                using var connection = new SqlConnection(_connectionString);
                var dateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
                assignProjectQuery = @"UPDATE [dbo].[AssignProject] 
                            SET UserId = @UserId, ProjectId = @ProjectId, StartDate = @StartDate, 
                            EndDate = @EndDate, DateTimeUpdated = @DateTimeUpdated
                            WHERE Id = @Id
                            ";

                var rowsInserted = connection.Execute(assignProjectQuery, new
                {
                    Id = req.Id,
                    UserId = req.UserId,
                    ProjectId = req.ProjectId,
                    StartDate = req.StartDate,
                    EndDate = req.EndDate,
                    DateTimeUpdated = dateTimeNow,
                });

                if (rowsInserted > 0)
                {

                    response.IsSuccess = true;
                    response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_UPDATE_SUCCESS;

                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = AssignProjectConstants.ASSIGN_PROJECT_UPDATE_FAILED;
                }
            }


            return response;
        }
    }
}
