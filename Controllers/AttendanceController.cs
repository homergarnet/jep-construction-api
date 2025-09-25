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
    public class AttendanceController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IEmployeeListService _employeeListService;
        private readonly IAttendanceService _iAttendanceService;

        public AttendanceController(IConfiguration configuration, IAttendanceService iAttendanceService)
        {
            _configuration = configuration;
            _iAttendanceService = iAttendanceService;
        }


        [Authorize]
        [HttpPost]
        [Route("time-in-out")]
        public async Task<IActionResult> CreateTimeInOut([FromBody] CreateTimeInOutRequest req)
        {
            try
            {

                var createTimeIn = await _iAttendanceService.CreateTimeInOut(req);
                if (!createTimeIn.IsSuccess && createTimeIn.ApiMessage.Equals(AttendanceConstants.CREATE_ATTENDANCE_FAILED))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(createTimeIn)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(createTimeIn)
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
        [Route("get-attendance-list")]
        public IActionResult GetAttendanceList(
            [FromQuery] string? keyword = "", [FromQuery] long? userId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getAttendanceList = _iAttendanceService.GetAttendanceList(keyword ?? "", userId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getAttendanceList)
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
        [HttpPut]
        [Route("update-attendance")]
        public async Task<IActionResult> UpdateAttendance([FromBody] UpdateAttendanceRequest req)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iAttendanceService.UpdateAttendance(req);
                if (!result.IsSuccess && result.ApiMessage.Equals(EmployeeListConstants.UPDATE_EMPLOYEE_FAILED))
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
        [HttpPut("soft-delete-attendance-by-id/{id}")]
        public IActionResult SoftDeleteAttendanceById(string id)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iAttendanceService.SoftDeleteAttendanceById(id);
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

    }
}
