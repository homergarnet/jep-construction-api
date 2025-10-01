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
    public class ClientRequestService : IClientRequestService
    {
        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private string crQuery = string.Empty;

        public ClientRequestService(IConfiguration configuration, Jep_ConstructionContext db)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
        }

        public ClientRequestResponse CreateClientRequest(CreateUpdateClientRequest req)
        {
            var response = new ClientRequestResponse
            {
                ClientRequestList = new List<ClientRequestDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };



            ClientRequest clientRequest = new ClientRequest();
            var email = req.Email?.Trim() ?? "";
            if (!string.IsNullOrEmpty(email) && new EmailAddressAttribute().IsValid(email))
            {
                clientRequest.Email = email;
            }
            else
            {
                response.IsSuccess = false;
                response.ApiMessage = ClientRequestConstants.INVALID_EMAIL_ADDRESS;
                return response;
     
            }

            clientRequest.ProjectName = req.ProjectName?.Trim() ?? "";
            clientRequest.Name = req.Name?.Trim() ?? "";
            clientRequest.Email = req.Email?.Trim() ?? "";
            clientRequest.MobileNumber = req.MobileNumber?.Trim() ?? "";
            clientRequest.Message = req.Message;
            clientRequest.DateTimeCreated = Common.DateTimeNow("Singapore Standard Time");
            db.ClientRequests.Add(clientRequest);
            db.SaveChanges();
            response.IsSuccess = true;
            response.ApiMessage = ClientRequestConstants.CREATE_CLIENT_REQUEST_SUCCESS;
            return response;

        }

        public ClientRequestResponse GetClientRequestById(long id)
        {
            var response = new ClientRequestResponse
            {
                ClientRequestList = new List<ClientRequestDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            try
            {
                if (id <= 0)
                {
                    response.ApiMessage = ClientRequestConstants.INVALID_CLIENT_REQUEST_ID;
                    return response;
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"SELECT * FROM [dbo].[ClientRequest] WHERE Id = @Id";

                    var clientRequest = connection.QueryFirstOrDefault<ClientRequestDto>(query, new { Id = id });

                    if (clientRequest != null)
                    {
                        response.ClientRequestList.Add(clientRequest);
                        response.TotalRecords = 1;
                        response.IsSuccess = true;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.ApiMessage = ClientRequestConstants.CLIENT_REQUEST_NOT_FOUND;
                    }
                }
            }
            catch (Exception ex)
            {
                response.ApiMessage = ex.Message;
            }

            return response;
        }

        public ClientRequestResponse GetClientRequestList(string keyword, int page, int pageSize)
        {

            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Equals("not/a"))
            {
                // no keyword filter → return all employees (paged)
                keyword = "";
            }
            var response = new ClientRequestResponse
            {
                ClientRequestList = new List<ClientRequestDto>(), // or UserList depending on your model
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
                        FROM [dbo].[ClientRequest]
                        WHERE (@Keyword = '' OR ProjectName LIKE '%' + @Keyword + '%') AND IsEnabled = 1";

                    var totalCount = connection.ExecuteScalar<long>(countQuery, new { Keyword = keyword });

                    // Paginated Data
                    var dataQuery = @"
                    SELECT *
                    FROM [dbo].[ClientRequest]
                    WHERE (@Keyword = '' OR ProjectName LIKE '%' + @Keyword + '%') AND IsEnabled = 1
                    ORDER BY Id DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

                    var data = connection.Query<ClientRequestDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize
                    }).ToList();

                    // Set response
                    response.ClientRequestList = data;
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

        public ClientRequestResponse SoftDeleteClientRequestById(string id)
        {
            var response = new ClientRequestResponse
            {
                ClientRequestList = new List<ClientRequestDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[ClientRequest] 
                            SET IsEnabled = 0
                            WHERE Id = @Id AND IsEnabled = 1";
                int rowsAffected = connection.Execute(sql, new { Id = id });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = ClientRequestConstants.SOFT_DELETE_CLIENT_REQUEST_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = ClientRequestConstants.SOFT_DELETE_CLIENT_REQUEST_FAILED;
                }

                return response;

            }
        }
    }
}
