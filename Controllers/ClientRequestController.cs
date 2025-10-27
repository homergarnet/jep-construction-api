using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Request;
using jep_construction_api.Services;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;
using System.Text.Json;

namespace jep_construction_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientRequestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IClientRequestService _iClientRequestService;
        private readonly SmtpSettings _smtpSettings;
        public ClientRequestController(IConfiguration configuration, IClientRequestService iClientRequestService, IOptions<SmtpSettings> smtpSettings)
        {
            _configuration = configuration;
            _iClientRequestService = iClientRequestService;
            _smtpSettings = smtpSettings.Value;
        }

        [Authorize]
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
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("App Sender", _smtpSettings.SmtpUsername));
            message.To.Add(MailboxAddress.Parse(request.To));
            message.Subject = request.Subject;

            // Create HTML email with footer and image
            var builder = new BodyBuilder();

            // Optional: use plain text fallback for email clients that don’t support HTML
            builder.TextBody = request.Body;

            // Path to your footer image (update this path)
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "jep_logo1.jpg");
            if (System.IO.File.Exists(imagePath))
            {
                var image = builder.LinkedResources.Add(imagePath);
                image.ContentId = MimeKit.Utils.MimeUtils.GenerateMessageId();

                builder.HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <p>{request.Body}</p>
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
                    <p>{request.Body}</p>
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

            try
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(_smtpSettings.SmtpServer, _smtpSettings.SmtpPort, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_smtpSettings.SmtpUsername, _smtpSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return Ok("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending email: {ex.Message}");
            }

        }

    }
}
