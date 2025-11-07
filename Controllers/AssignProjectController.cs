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
    public class AssignProjectController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAssignProjectService _iAssignProjectService;
        public AssignProjectController(IConfiguration configuration, IAssignProjectService iAssignProjectService)
        {
            _configuration = configuration;
            _iAssignProjectService = iAssignProjectService;
        }

        [Authorize]
        [HttpPost]
        [Route("create-assign-project")]
        public IActionResult CreateAssignProject([FromBody] AssignProjectCreateUpdateRequest req)
        {
            try
            {

                var createAssignProject = _iAssignProjectService.CreateAssignProject(req);
                if (!createAssignProject.IsSuccess &&
                    createAssignProject.ApiMessage.Equals(AssignProjectConstants.ASSIGN_PROJECT_NOT_YET_DONE))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(createAssignProject)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(createAssignProject)
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
        [Route("get-assign-project-list")]
        public IActionResult GetAssignProjectList(
            [FromQuery] string? keyword = "", [FromQuery] long? userId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getAssignProjectList = _iAssignProjectService.GetAssignProjectList(keyword ?? "", userId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getAssignProjectList)
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
        [Route("get-cname-pname-list")]
        public IActionResult GetCNamePNameList(
            [FromQuery] string? keyword = "", [FromQuery] long? userId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getAssignProjectList = _iAssignProjectService.GetCNamePNameList(keyword ?? "", userId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getAssignProjectList)
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
        [Route("get-assign-project-by-id")]
        public IActionResult GetAssignProjectById([FromQuery] long id = 0)
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getAssignProject = _iAssignProjectService.GetAssignProjectById(id);
                if (!getAssignProject.IsSuccess && getAssignProject.ApiMessage.Equals(AssignProjectConstants.ASSIGN_PROJECT_INVALID_ID)
                    || getAssignProject.ApiMessage.Equals(AssignProjectConstants.ASSIGN_PROJECT_NOT_FOUND))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(getAssignProject)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getAssignProject)
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
        [Route("update-assign-project")]
        public async Task<IActionResult> UpdateAssignProject([FromBody] AssignProjectCreateUpdateRequest req)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iAssignProjectService.UpdateAssignProject(req);
                if (!result.IsSuccess && result.ApiMessage.Equals(AssignProjectConstants.ASSIGN_PROJECT_UPDATE_FAILED) || 
                    result.ApiMessage.Equals(AssignProjectConstants.ASSIGN_PROJECT_NOT_YET_DONE))
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
        [HttpPut("soft-delete-assign-project-by-id/{id}")]
        public IActionResult SoftDeleteAssignProjectById(string id)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iAssignProjectService.SoftDeleteAssignProjectById(id);
                if (!result.IsSuccess && result.ApiMessage.Equals(AssignProjectConstants.ASSIGN_PROJECT_SOFT_DELETE_FAILED))
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
