using Dapper;
using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Models;
using jep_construction_api.Request;
using jep_construction_api.Response;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MimeKit;
using System.ComponentModel.DataAnnotations;

namespace jep_construction_api.Services
{
    public class ClientRequestService : IClientRequestService
    {
        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private string crQuery = string.Empty;
        private readonly SmtpSettings _smtpSettings;
        public ClientRequestService(IConfiguration configuration, Jep_ConstructionContext db, IOptions<SmtpSettings> smtpSettings)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
            _smtpSettings = smtpSettings.Value;
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

        public async Task<ClientRequestResponse> SendEmail(EmailRequest req)
        {
            var response = new ClientRequestResponse
            {
                ClientRequestList = new List<ClientRequestDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("App Sender", _smtpSettings.SmtpUsername));
            message.To.Add(MailboxAddress.Parse(req.To));
            message.Subject = req.Subject;

            // Create HTML email with footer and image
            var builder = new BodyBuilder();

            // Optional: use plain text fallback for email clients that don’t support HTML
            builder.TextBody = req.Body;

            // Path to your footer image (update this path)
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "jep_logo1.jpg");
            if (System.IO.File.Exists(imagePath))
            {
                var image = builder.LinkedResources.Add(imagePath);
                image.ContentId = MimeKit.Utils.MimeUtils.GenerateMessageId();

                builder.HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <p>{req.Body}</p>
                    <br/>
                    <hr style='border:none; border-top:1px solid #ddd; margin:20px 0;'/>
                    <div style='text-align:center;'>
                        <img src='cid:{image.ContentId}' alt='Footer Image' style='width:120px; height:auto;'/>
                        <p style='font-size:12px; color:#888; margin-top:10px;'>
                            © 2025 Jep Construction. All rights reserved.<br/>
                            <a href='https://jepconstruction-001-site1.stempurl.com' style='color:#007bff; text-decoration:none;'>Visit our website</a>
                        </p>
                    </div>
                </div>";
            }
            else
            {
                // fallback if image is missing
                builder.HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <p>{req.Body}</p>
                    <br/>
                    <hr style='border:none; border-top:1px solid #ddd; margin:20px 0;'/>
                    <div style='text-align:center;'>
                        <p style='font-size:12px; color:#888;'>
                            © 2025 Jep Construction. All rights reserved.<br/>
                            <a href='https://jepconstruction-001-site1.stempurl.com' style='color:#007bff; text-decoration:none;'>Visit our website</a>
                        </p>
                    </div>
                </div>";
            }

            message.Body = builder.ToMessageBody();
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(_smtpSettings.SmtpServer, _smtpSettings.SmtpPort, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_smtpSettings.SmtpUsername, _smtpSettings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[ClientRequest] 
                            SET HasReply = 1
                            WHERE Id = @Id AND IsEnabled = 1";
                int rowsAffected = connection.Execute(sql, new { Id = req.Id });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    //response.ApiMessage = ClientRequestConstants.SOFT_DELETE_CLIENT_REQUEST_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    //response.ApiMessage = ClientRequestConstants.SOFT_DELETE_CLIENT_REQUEST_FAILED;
                }

            }
            response.IsSuccess = true;
            response.ApiMessage = "Email sent successfully!";
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
