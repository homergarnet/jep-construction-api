using jep_construction_api.Constants;
using jep_construction_api.Library;
using jep_construction_api.Request;
using jep_construction_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            [FromQuery] string? keyword = "", [FromQuery] long userId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getMessageList = _iMessageService.GetMessageList(keyword ?? "", userId, page, pageSize);

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

    }
}
