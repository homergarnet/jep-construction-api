using jep_construction_api.Constants;
using jep_construction_api.DTOS;
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
    public class EmployeeListController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IEmployeeListService _employeeListService;
        public EmployeeListController(IConfiguration configuration, IEmployeeListService employeeListService)
        {
            _configuration = configuration;
            _employeeListService = employeeListService;
        }

        [Authorize]
        [HttpPost]
        [Route("create-employee")]
        public IActionResult CreateEmployee([FromBody] CreateUpdateEmployeeRequest createEmployeeReq)
        {
            try
            {

                var createAccount = _employeeListService.CreateEmployee(createEmployeeReq);
                if (!createAccount.IsSuccess && createAccount.ApiMessage.Equals(EmployeeListConstants.EMAIL_ALREADY_EXIST) 
                    || createAccount.ApiMessage.Equals(AuthConstants.INVALID_EMAIL_ADDRESS))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(createAccount)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(createAccount)
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
        [Route("get-employee-list")]
        public IActionResult GetEmployeeList(
            [FromQuery] string? keyword = "", [FromQuery] string? accountType = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getEmployeeList = _employeeListService.GetEmployeeList(keyword ?? "", page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getEmployeeList)
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
        [Route("get-employee-by-id")]
        public IActionResult GetEmployeeById([FromQuery] long id = 0)
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getEmployee = _employeeListService.GetEmployeeById(id);
                if (!getEmployee.IsSuccess && getEmployee.ApiMessage.Equals(EmployeeListConstants.INVALID_EMPLOYEE_ID) 
                    || getEmployee.ApiMessage.Equals(EmployeeListConstants.EMPLOYEE_NOT_FOUND))
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
        [HttpPut]
        [Route("update-employee")]
        public async Task<IActionResult> UpdateEmployee([FromBody] CreateUpdateEmployeeRequest updateEmployeeReq)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _employeeListService.UpdateEmployee(updateEmployeeReq);
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
        [HttpPut("soft-delete-employee-by-id/{id}")]
        public IActionResult SoftDeleteEmployeeById(string id)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _employeeListService.SoftDeleteEmployeeById(id);
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
