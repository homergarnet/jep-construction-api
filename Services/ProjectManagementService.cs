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
    public class ProjectManagementService : IProjectManagementService
    {
        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private string pmQuery = string.Empty;
        public ProjectManagementService(IConfiguration configuration, Jep_ConstructionContext db)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
        }

        public ProjectManagementResponse CreateProjectManagement(CreateUpdateProjectManagementRequest req)
        {

            var response = new ProjectManagementResponse
            {
                ProjectManagementList = new List<ProjectManagementDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.UserId = req.UserId;
            projectManagement.ProjectName = req.ProjectName?.Trim() ?? "";
            projectManagement.StartDate = req.StartDate;
            projectManagement.EndDate = req.EndDate;
            projectManagement.Budget = req.Budget;
            projectManagement.Location = req.Location?.Trim() ?? "";
            projectManagement.Description = req.Description?.Trim() ?? "";
            projectManagement.CompletionStatus = req.CompletionStatus;
            projectManagement.DateTimeCreated = Common.DateTimeNow("Singapore Standard Time");
            db.ProjectManagements.Add(projectManagement);
            db.SaveChanges();
            response.IsSuccess = true;
            response.ApiMessage = ProjectManagementConstants.CREATE_PROJECT_MANAGEMENT_ACCOUNT_SUCCESS;
            return response;

        }

        public ProjectManagementResponse GetProjectManagementById(long id)
        {
            var response = new ProjectManagementResponse
            {
                ProjectManagementList = new List<ProjectManagementDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            try
            {
                if (id <= 0)
                {
                    response.ApiMessage = "Invalid Project ID.";
                    return response;
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT pm.Id AS ProjectId, pm.ProjectName, pm.StartDate, pm.EndDate, pm.Budget, pm.Location, pm.Description, 
                    pm.CompletionStatus,
                    (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                    FROM [dbo].[ProjectManagement] pm
                    INNER JOIN [dbo].[User] u ON u.Id = pm.UserId
                    WHERE pm.Id = @Id";

                    var projectManagement = connection.QueryFirstOrDefault<ProjectManagementDto>(query, new { Id = id });

                    if (projectManagement != null)
                    {
                        response.ProjectManagementList.Add(projectManagement);
                        response.TotalRecords = 1;
                        response.IsSuccess = true;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.ApiMessage = "Project id not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.ApiMessage = ex.Message;
            }

            return response;

        }

        public ProjectManagementResponse GetProjectManagementList(string keyword, long? userId, int page, int pageSize)
        {
            var response = new ProjectManagementResponse
            {
                ProjectManagementList = new List<ProjectManagementDto>(), // or UserList depending on your model
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            try
            {
                keyword = keyword ?? string.Empty;

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Total Count
                    var countQuery = @"
                        SELECT COUNT(*)
                        FROM [dbo].[ProjectManagement]
                        WHERE (@Keyword = '' OR ProjectName LIKE '%' + @Keyword + '%')";

                    var totalCount = connection.ExecuteScalar<long>(countQuery, new { Keyword = keyword });
                    var dataQuery = "";
                    // if not admin
                    if (userId != 0)
                    {
                        // Paginated Data
                        dataQuery = @"
                        SELECT pm.Id AS ProjectId, pm.ProjectName, pm.StartDate, pm.EndDate, pm.Budget, pm.Location, pm.Description, pm.CompletionStatus,
                               (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[ProjectManagement] pm
                        INNER JOIN [dbo].[User] u ON u.Id = pm.UserId
                        WHERE (@Keyword = '' OR pm.ProjectName LIKE '%' + @Keyword + '%') 
                          AND pm.UserId = @UserId
                        ORDER BY pm.Id DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";

                        var data = connection.Query<ProjectManagementDto>(dataQuery, new
                        {
                            Keyword = keyword,
                            Offset = (page - 1) * pageSize,
                            PageSize = pageSize,
                            UserId = userId   // ✅ added
                        }).ToList();

                        response.ProjectManagementList = data;
                    }
                    // if admin
                    else
                    {
                        // Paginated Data
                        dataQuery = @"
                        SELECT pm.Id AS ProjectId, pm.ProjectName, pm.StartDate, pm.EndDate, pm.Budget, pm.Location, pm.Description, pm.CompletionStatus,
                        (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[ProjectManagement] pm
                        INNER JOIN [dbo].[User] u ON u.Id = pm.UserId
                        WHERE (@Keyword = '' OR pm.ProjectName LIKE '%' + @Keyword + '%')
                        ORDER BY pm.Id DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";
                        var data = connection.Query<ProjectManagementDto>(dataQuery, new
                        {
                            Keyword = keyword,
                            Offset = (page - 1) * pageSize,
                            PageSize = pageSize
                        }).ToList();

                        // Set response
                        response.ProjectManagementList = data;
                    }

                    response.TotalRecords = totalCount;
                    response.IsSuccess = true;

                }
            }
            catch (Exception ex)
            {
                response.ApiMessage = ex.Message;
            }

            return response;
        }

        public ProjectManagementResponse SoftDeleteProjectManagementById(string id)
        {
            var response = new ProjectManagementResponse
            {
                ProjectManagementList = new List<ProjectManagementDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[ProjectManagement] 
                            SET IsEnabled = 0
                            WHERE Id = @Id";
                int rowsAffected = connection.Execute(sql, new { Id = id });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = ProjectManagementConstants.SOFT_DELETE_PROJECT_MANAGEMENT_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = ProjectManagementConstants.SOFT_DELETE_PROJECT_MANAGEMENT_FAILED;
                }

                return response;

            }
        }

        public ProjectManagementResponse UpdateProjectManagement(CreateUpdateProjectManagementRequest req)
        {
            var response = new ProjectManagementResponse
            {
                ProjectManagementList = new List<ProjectManagementDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using var connection = new SqlConnection(_connectionString);
            var dateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
            pmQuery = @"UPDATE [dbo].[ProjectManagement] 
                            SET ProjectName = @ProjectName, StartDate = @StartDate, EndDate = @EndDate, Budget = @Budget,
                            Location = @Location, Description = @Description, CompletionStatus = @CompletionStatus,
                            DateTimeUpdated = @DateTimeUpdated
                            WHERE Id = @Id
                            ";

            var rowsInserted = connection.Execute(pmQuery, new
            {
                Id = req.Id,
                ProjectName = req.ProjectName?.Trim() ?? "",
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                Budget = req.Budget,
                Location = req.Location?.Trim() ?? "",
                Description = req.Description?.Trim() ?? "",
                CompletionStatus = req.CompletionStatus,
                DateTimeUpdated = dateTimeNow,
            });

            if (rowsInserted > 0)
            {

                response.IsSuccess = true;
                response.ApiMessage = ProjectManagementConstants.UPDATE_PROJECT_MANAGEMENT_SUCCESS;

            }
            else
            {
                response.IsSuccess = false;
                response.ApiMessage = ProjectManagementConstants.UPDATE_PROJECT_MANAGEMENT_FAILED;
            }

            return response;
        }
    }
}
