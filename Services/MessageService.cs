using barangay_crime_compliant_api.Hubs;
using Dapper;
using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Models;
using jep_construction_api.Request;
using jep_construction_api.Response;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;

namespace jep_construction_api.Services
{
    public class MessageService : IMessageService
    {

        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly IHubContext<MessageHub> _messageHubContext;
        private readonly string _connectionString;

        private string messageQuery = string.Empty;

        public MessageService(IConfiguration configuration, Jep_ConstructionContext db, IHubContext<MessageHub> messageHubContext)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
            _messageHubContext = messageHubContext;
        }

        public MessageResponse CreateMessage(CreateMessageRequest req)
        {
            var response = new MessageResponse
            {
                MessageList = new List<MessageDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            // user.Id = item.Id;
            Models.Message message = new Models.Message();
            message.UserId = req.UserId;
            message.SenderId = req.SenderId;
            message.ReceiverId = req.ReceiverId;
            message.Message1 = req.Message?.Trim() ?? "";
            message.DateTimeCreated = Common.DateTimeNow("Singapore Standard Time");
            db.Messages.Add(message);
            db.SaveChanges();
            response.IsSuccess = true;
            response.ApiMessage = MessageConstants.CREATE_MESSAGE_SUCCESS;
            var roomId = MessageConstants.MESSAGE_ROOM_ID; // determine room id as per your app logic
            // Since it's async, you can fire-and-forget like this (safe in non-critical cases)
            _ = _messageHubContext.Clients.Group(roomId)
                .SendAsync("SendMessage", roomId, req.UserId, req.SenderId, req.ReceiverId, req.Message);

            return response;

        }

        public MessageResponse GetMessageList(string keyword, long userId, int page, int pageSize)
        {
            var response = new MessageResponse
            {
                MessageList = new List<MessageDto>(), // or UserList depending on your model
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
                        FROM [dbo].[Message]
                        WHERE (@Keyword = '' OR Message LIKE '%' + @Keyword + '%') AND (SenderId = @UserId OR ReceiverId = @UserId)";

                var totalCount = connection.ExecuteScalar<long>(countQuery, new
                {
                    Keyword = keyword,
                    UserId = userId
                });

                // Paginated Data
                var dataQuery = @"
                SELECT *
                FROM [dbo].[Message]
                WHERE (@Keyword = '' OR Message LIKE '%' + @Keyword + '%') AND (SenderId = @UserId OR ReceiverId = @UserId)
                ORDER BY Id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

                var data = connection.Query<MessageDto>(dataQuery, new
                {
                    Keyword = keyword,
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize,
                    UserId = userId
                }).ToList();

                // Set response
                response.MessageList = data;
                response.TotalRecords = totalCount;
                response.IsSuccess = true;
            }

            return response;
        }
    }
}
