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
                if (!createProjectManagement.IsSuccess && createProjectManagement.ApiMessage.Equals(ProjectManagementConstants.PROJECT_NAME_ALREADY_EXIST))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
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
                if (!getProjectManagement.IsSuccess &&
                    getProjectManagement.ApiMessage.Equals(ProjectManagementConstants.INVALID_PROJECT_ID)
                    || getProjectManagement.ApiMessage.Equals(ProjectManagementConstants.PROJECT_ID_NOT_FOUND))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(getProjectManagement)
                    };
                }
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
        [HttpGet]
        [Route("get-project-id-by-cname-pname")]
        public IActionResult GetProjectIdByCNamePName([FromQuery] string cName = "", [FromQuery] string pName = "")
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getProjectIdByCNamePName = _iProjectManagementService.GetProjectIdByCNamePName(cName, pName);
                if (!getProjectIdByCNamePName.IsSuccess &&
                    getProjectIdByCNamePName.ApiMessage.Equals(ProjectManagementConstants.INVALID_PARAMETERS)
                    || getProjectIdByCNamePName.ApiMessage.Equals(ProjectManagementConstants.PROJECT_ID_NOT_FOUND))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(getProjectIdByCNamePName)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getProjectIdByCNamePName)
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

            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iProjectManagementService.UpdateProjectManagement(req);
                if (!result.IsSuccess && result.ApiMessage.Equals(ProjectManagementConstants.UPDATE_PROJECT_MANAGEMENT_FAILED))
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
        [HttpPut("soft-delete-project-management-by-id/{id}")]
        public IActionResult SoftDeleteProjectManagementById(string id)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iProjectManagementService.SoftDeleteProjectManagementById(id);
                if (!result.IsSuccess && result.ApiMessage.Equals(ProjectManagementConstants.SOFT_DELETE_PROJECT_MANAGEMENT_FAILED))
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
