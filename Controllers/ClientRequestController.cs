using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Request;
using jep_construction_api.Response;
using jep_construction_api.Services;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Ocsp;
using System.Net.Mail;
using System.Text.Json;
using Twilio.Http;

namespace jep_construction_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientRequestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IClientRequestService _iClientRequestService;

        public ClientRequestController(IConfiguration configuration, IClientRequestService iClientRequestService)
        {
            _configuration = configuration;
            _iClientRequestService = iClientRequestService;

        }

        [HttpPost]
        [Route("create-client-request")]
        public IActionResult CreateClientRequest([FromBody] CreateUpdateClientRequest req)
        {
            try
            {

                var createClient = _iClientRequestService.CreateClientRequest(req);
                if (!createClient.IsSuccess && createClient.ApiMessage.Equals(EmployeeListConstants.EMAIL_ALREADY_EXIST)
                    || createClient.ApiMessage.Equals(AuthConstants.INVALID_EMAIL_ADDRESS))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(createClient)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(createClient)
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
        [Route("get-client-request-list")]
        public IActionResult GetClientRequestList(
            [FromQuery] string? keyword = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getClientReviewList = _iClientRequestService.GetClientRequestList(keyword ?? "", page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getClientReviewList)
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
        [Route("get-client-request-by-id")]
        public IActionResult GetClientRequestById([FromQuery] long id = 0)
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getEmployee = _iClientRequestService.GetClientRequestById(id);
                if (!getEmployee.IsSuccess && getEmployee.ApiMessage.Equals(ClientRequestConstants.INVALID_CLIENT_REQUEST_ID)
                    || getEmployee.ApiMessage.Equals(ClientRequestConstants.CLIENT_REQUEST_NOT_FOUND))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(getEmployee)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getEmployee)
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
        [HttpPut("soft-delete-client-request-by-id/{id}")]
        public IActionResult SoftDeleteClientRequestById(string id)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iClientRequestService.SoftDeleteClientRequestById(id);
                if (!result.IsSuccess && result.ApiMessage.Equals(EmployeeListConstants.SOFT_DELETE_EMPLOYEE_FAILED))
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

        [Authorize]
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest req)
        {

            try
            {

                var sendEmail = await _iClientRequestService.SendEmail(req);
                if (!sendEmail.IsSuccess)
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(sendEmail)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(sendEmail)
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
