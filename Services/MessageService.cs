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
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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

            var message = new Models.Message
            {
                UserId = req.UserId,
                SenderId = req.SenderId,
                ReceiverId = req.ReceiverId,
                Message1 = req.Message?.Trim() ?? "",
                DateTimeCreated = Common.DateTimeNow("Singapore Standard Time")
            };

            db.Messages.Add(message);
            db.SaveChanges(); // ✅ message.Id gets generated after this

            // ✅ Add the message to MessageList in response
            response.MessageList.Add(new MessageDto
            {
                Id = message.Id,
                UserId = message.UserId,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Message = message.Message1,
                DateTimeCreated = Common.DateTimeNow("Singapore Standard Time"),
                ProfileImage = db.Users.Where(z => z.Id == message.SenderId).Select(z => z.ProfileImage).FirstOrDefault(),         // Fill if needed
                IsEnabled = true           // Fill based on your logic
            });

            response.IsSuccess = true;
            response.ApiMessage = MessageConstants.CREATE_MESSAGE_SUCCESS;

            // Optional: Send to SignalR clients
            var roomId = MessageConstants.MESSAGE_ROOM_ID;
            _ = _messageHubContext.Clients.Group(roomId)
                .SendAsync("ReceiveMessage", roomId, req.UserId, req.SenderId, req.ReceiverId, req.Message);

            return response;
        }

        public ConvoResponse GetConvoRowList(long? userId, int page, int pageSize)
        {
            var response = new ConvoResponse
            {
                ConvoList = new List<ConvoDto>(), // or UserList depending on your model
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Total Count
                var countQuery = @"
                WITH Conversations AS (
                    SELECT 
                        CASE 
                            WHEN SenderId = @UserId THEN ReceiverId 
                            ELSE SenderId 
                        END AS ConvoUserId,
                        ROW_NUMBER() OVER (
                            PARTITION BY CASE 
                                            WHEN SenderId = @UserId THEN ReceiverId 
                                            ELSE SenderId 
                                         END
                            ORDER BY Id DESC
                        ) AS rn
                    FROM Message
                    WHERE 
                        (SenderId = @UserId OR ReceiverId = @UserId)
                        AND IsEnabled = 1
                )
                SELECT COUNT(*) 
                FROM Conversations c
                INNER JOIN [dbo].[User] u 
                    ON u.Id = c.ConvoUserId AND u.IsEnabled = 1
                WHERE c.rn = 1;
                ";

                var totalCount = connection.ExecuteScalar<long>(countQuery, new
                {
                    UserId = userId
                });

                // Paginated Data
                var dataQuery = @"
                WITH Conversations AS (
                    SELECT 
                        CASE 
                            WHEN SenderId = @UserId THEN ReceiverId 
                            ELSE SenderId 
                        END AS ConvoUserId,
                        Id,
                        Message,
                        IsRead,
                        ReceiverId,
                        ROW_NUMBER() OVER (
                            PARTITION BY CASE 
                                            WHEN SenderId = @UserId THEN ReceiverId 
                                            ELSE SenderId 
                                         END
                            ORDER BY Id DESC
                        ) AS rn
                    FROM Message
                    WHERE 
                        (SenderId = @UserId OR ReceiverId = @UserId)
                        AND IsEnabled = 1
                )
                SELECT 
                    (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ConvoName,
                    u.ProfileImage AS ConvoImage,
                    (SELECT COUNT(*) 
                     FROM Message m 
                     WHERE m.SenderId = c.ConvoUserId 
                       AND m.ReceiverId = @UserId 
                       AND m.IsRead = 0 
                       AND m.IsEnabled = 1) AS UnreadCount,
                    c.Message AS LastMessage,
                    c.ConvoUserId
                FROM Conversations c
                INNER JOIN [dbo].[User] u 
                    ON u.Id = c.ConvoUserId AND u.IsEnabled = 1
                WHERE c.rn = 1
                ORDER BY c.Id DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;
                ";

                var data = connection.Query<ConvoDto>(dataQuery, new
                {
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize,
                    UserId = userId
                }).ToList();

                // Set response
                response.ConvoList = data;
                response.TotalRecords = totalCount;
                response.IsSuccess = true;
            }

            return response;
        }

        public MessageResponse GetMessageList(string keyword, long userId, long convoUserId, string orderBy, int page, int pageSize)
        {

            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Equals("not/a"))
            {
                // no keyword filter → return all employees (paged)
                keyword = "";
            }

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
                        FROM [dbo].[Message] m
                INNER JOIN [dbo].[User] u ON u.Id = m.SenderId
                WHERE (@Keyword = '' OR m.Message LIKE '%' + @Keyword + '%') AND 
                ((m.SenderId = @UserId AND m.ReceiverId = @ConvoUserId)
                OR (m.SenderId = @ConvoUserId AND m.ReceiverId = @UserId))";

                var totalCount = connection.ExecuteScalar<long>(countQuery, new
                {
                    Keyword = keyword,
                    UserId = userId,
                    ConvoUserId = convoUserId
                });

                // Validate orderBy to prevent SQL Injection
                string safeOrderBy = orderBy?.ToUpper() == "DESC" ? "DESC" : "ASC";
                string dataQuery = "";
                if (page == 112 && pageSize == 10)
                {
                    dataQuery = $@"
                        SELECT *
                        FROM (
                            SELECT TOP (@PageSize)
                            m.Id, m.UserId, m.SenderId, m.ReceiverId, m.Message, m.DateTimeCreated, u.ProfileImage
                            FROM [dbo].[Message] m
                            INNER JOIN [dbo].[User] u ON u.Id = m.SenderId
                            WHERE (@Keyword = '' OR m.Message LIKE '%' + @Keyword + '%')
                            AND (
                                (m.SenderId = @UserId AND m.ReceiverId = @ConvoUserId)
                                OR
                                (m.SenderId = @ConvoUserId AND m.ReceiverId = @UserId)
                            )
                            ORDER BY m.DateTimeCreated DESC  -- Get the latest messages
                        ) AS LatestMessages
                        ORDER BY DateTimeCreated ASC;        -- Display oldest to newest
                    ";
                    var data = connection.Query<MessageDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        PageSize = pageSize,
                        UserId = userId,
                        ConvoUserId = convoUserId
                    }).ToList();

                    // Set response
                    response.MessageList = data;
                    response.TotalRecords = totalCount;
                    response.IsSuccess = true;
                }
                else
                {
                    //page = page - 1;
                    // Paginated Data
                    dataQuery = $@"
                    SELECT m.Id, m.UserId, m.SenderId, m.ReceiverId, m.Message, m.DateTimeCreated, u.ProfileImage
                    FROM [dbo].[Message] m
                    INNER JOIN [dbo].[User] u ON u.Id = m.SenderId
                    WHERE (@Keyword = '' OR m.Message LIKE '%' + @Keyword + '%')
                      AND ((m.SenderId = @UserId AND m.ReceiverId = @ConvoUserId)
                        OR (m.SenderId = @ConvoUserId AND m.ReceiverId = @UserId))
                    ORDER BY m.Id {safeOrderBy}
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";
                    var data = connection.Query<MessageDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                        UserId = userId,
                        ConvoUserId = convoUserId
                    }).ToList();

                    // Set response
                    response.MessageList = data;
                    response.TotalRecords = totalCount;
                    response.IsSuccess = true;
                }

            }

            return response;
        }

        public EmployeeListResponse GetMessageUserList(string keyword, long userId, int page, int pageSize)
        {

            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Equals("not/a"))
            {
                // no keyword filter → return all employees (paged)
                keyword = "";
            }
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
                string countQuery = "";
                long totalCount = 0;


                countQuery = @"
                        SELECT COUNT(*)
                        FROM [dbo].[User]
                        WHERE (@Keyword = '' OR Email LIKE '%' + @Keyword + '%' OR Status LIKE '%' + @Keyword + '%') 
                        AND Id != @UserId AND IsEnabled = 1";

                totalCount = connection.ExecuteScalar<long>(countQuery, new
                {
                    Keyword = keyword,
                    UserId = userId
                });



                string dataQuery = "";
                // Paginated Data

                dataQuery = @"
                    SELECT *
                    FROM [dbo].[User]
                    WHERE (@Keyword = '' OR Email LIKE '%' + @Keyword + '%' OR Status LIKE '%' + @Keyword + '%') AND Id != @UserId 
                    AND IsEnabled = 1
                    ORDER BY Id DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";


                var data = connection.Query<UserDto>(dataQuery, new
                {
                    Keyword = keyword,
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize,
                    UserId = userId
                }).ToList();

                // Set response
                response.UserList = data;
                response.TotalRecords = totalCount;
                response.IsSuccess = true;
            }

            return response;

        }

        public MessageResponse SetReadById(long senderId, long userId)
        {
            var response = new MessageResponse
            {
                MessageList = new List<MessageDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[Message] 
                            SET IsRead = 1
                            WHERE UserId = @SenderId AND ReceiverId = @ReceiverId";
                int rowsAffected = connection.Execute(sql, new { SenderId = senderId, ReceiverId = userId });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = MessageConstants.SET_READ_BY_ID_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = MessageConstants.SET_READ_BY_ID_FAILED;
                }

                return response;

            }
        }
    }
}
