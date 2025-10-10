using jep_construction_api.Constants;
using jep_construction_api.Library;
using jep_construction_api.Models;
using jep_construction_api.Request;
using jep_construction_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.Text.Json;

namespace jep_construction_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IMessageService _iMessageService;

        public MessageController(IConfiguration configuration, IMessageService iMessageService)
        {
            _configuration = configuration;
            _iMessageService = iMessageService;
        }

        [Authorize]
        [HttpPost]
        [Route("create-message")]
        public IActionResult CreateMessage([FromBody] CreateMessageRequest req)
        {
            try
            {

                var createMessage = _iMessageService.CreateMessage(req);
                //if (!createMessage.IsSuccess && createMessage.ApiMessage.Equals(EmployeeListConstants.EMAIL_ALREADY_EXIST)
                //    || createMessage.ApiMessage.Equals(AuthConstants.INVALID_EMAIL_ADDRESS))
                //{
                //    return new ContentResult
                //    {
                //        StatusCode = 400,
                //        ContentType = "application/json",
                //        Content = JsonSerializer.Serialize(createMessage)
                //    };
                //}
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(createMessage)
                };

            }

            catch (Exception ex)
            {
                return new ContentResult
                {
                    StatusCode = 500,
                    ContentType = "text/html",
                    Content = Common.GetFormattedExceptionMessage(ex)
                };
            }

        }

        [Authorize]
        [HttpGet]
        [Route("get-message-list")]
        public IActionResult GetMessageList(
            [FromQuery] string? keyword = "", [FromQuery] long convoUserId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userId = Convert.ToInt64(User.FindFirst("UserId")?.Value);
                var getMessageList = _iMessageService.GetMessageList(keyword ?? "", userId, convoUserId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getMessageList)
                };

            }

            catch (Exception ex)
            {
                return new ContentResult
                {
                    StatusCode = 500,
                    ContentType = "text/html",
                    Content = Common.GetFormattedExceptionMessage(ex)
                };
            }

        }

        [Authorize]
        [HttpGet]
        [Route("get-convo-row-list")]
        public IActionResult GetConvoRowList(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userId = Convert.ToInt64(User.FindFirst("UserId")?.Value);
                var getConvoRowList = _iMessageService.GetConvoRowList(userId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getConvoRowList)
                };

            }

            catch (Exception ex)
            {
                return new ContentResult
                {
                    StatusCode = 500,
                    ContentType = "text/html",
                    Content = Common.GetFormattedExceptionMessage(ex)
                };
            }

        }

        [Authorize]
        [HttpGet]
        [Route("get-message-user-list")]
        public IActionResult GetMessageUserList(
            [FromQuery] string? keyword = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userId = Convert.ToInt64(User.FindFirst("UserId")?.Value);
                var getMessageUserList = _iMessageService.GetMessageUserList(keyword ?? "", userId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getMessageUserList)
                };

            }

            catch (Exception ex)
            {
                return new ContentResult
                {
                    StatusCode = 500,
                    ContentType = "text/html",
                    Content = Common.GetFormattedExceptionMessage(ex)
                };
            }

        }

        [Authorize]
        [HttpPut("set-read-by-id/{senderId}")]
        public IActionResult SetReadById(long senderId)
        {
            try
            {
                var userId = Convert.ToInt64(User.FindFirst("UserId")?.Value);
                var result = _iMessageService.SetReadById(senderId, userId);
                if (!result.IsSuccess && result.ApiMessage.Equals(MessageConstants.SET_READ_BY_ID_FAILED))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(result)
                    };
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return new ContentResult
                {
                    StatusCode = 500,
                    ContentType = "text/html",
                    Content = Common.GetFormattedExceptionMessage(ex)
                };
            }
        }

    }
}
