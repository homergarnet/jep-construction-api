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
    public class ProjectManagementController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IProjectManagementService _iProjectManagementService;
        public ProjectManagementController(IConfiguration configuration, IProjectManagementService iProjectManagementService)
        {
            _configuration = configuration;
            _iProjectManagementService = iProjectManagementService;
        }

        [Authorize]
        [HttpPost]
        [Route("create-project-management")]
        public IActionResult CreateProjectManagement([FromBody] CreateUpdateProjectManagementRequest req)
        {
            try
            {

                var createProjectManagement = _iProjectManagementService.CreateProjectManagement(req);
                if (!createProjectManagement.IsSuccess)
                {
                    return new ContentResult
                    {
                        StatusCode = 500,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(createProjectManagement)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(createProjectManagement)
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
        [Route("get-project-management-list")]
        public IActionResult GetProjectManagementList(
            [FromQuery] string? keyword = "", [FromQuery] long? userId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getProjectManagementList = _iProjectManagementService.GetProjectManagementList(keyword ?? "", userId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getProjectManagementList)
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
        [Route("get-project-management-by-id")]
        public IActionResult GetProjectManagementById([FromQuery] long id = 0)
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getProjectManagement = _iProjectManagementService.GetProjectManagementById(id);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getProjectManagement)
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
        [Route("update-project-management")]
        public async Task<IActionResult> UpdateProjectManagement([FromBody] CreateUpdateProjectManagementRequest req)
        {
            var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
            var result = _iProjectManagementService.UpdateProjectManagement(req);
            if (!result.IsSuccess)
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

        [Authorize]
        [HttpPut("soft-delete-project-management-by-id/{id}")]
        public IActionResult SoftDeleteProjectManagementById(string id)
        {

            var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
            var result = _iProjectManagementService.SoftDeleteProjectManagementById(id);
            if (!result.IsSuccess)
            {
                return new ContentResult
                {
                    StatusCode = 500,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(result)
                };
            }
            return Ok(result);
        }

    }
}
