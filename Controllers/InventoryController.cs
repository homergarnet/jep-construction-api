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
    public class InventoryController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IInventoryService _iInventoryService;

        public InventoryController(IConfiguration configuration, IInventoryService iInventoryService)
        {
            _configuration = configuration;
            _iInventoryService = iInventoryService;
        }

        [Authorize]
        [HttpPost]
        [Route("create-inventory")]
        public IActionResult CreateInventory([FromBody] CreateUpdateInventoryRequest req)
        {
            try
            {

                var createInventory = _iInventoryService.CreateInventory(req);
                if (!createInventory.IsSuccess && createInventory.ApiMessage.Equals(InventoryConstants.INVENTORY_IN_USER_ALREADY_EXIST))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(createInventory)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(createInventory)
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
        [Route("get-inventory-list")]
        public IActionResult GetInventoryList(
            [FromQuery] string? keyword = "", [FromQuery] long? userId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getInventoryList = _iInventoryService.GetInventoryList(keyword ?? "", userId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getInventoryList)
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
        [Route("get-inventory-by-id")]
        public IActionResult GetInventoryById([FromQuery] long id = 0)
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getInventory = _iInventoryService.GetInventoryById(id);
                if (!getInventory.IsSuccess && getInventory.ApiMessage.Equals(InventoryConstants.INVALID_INVENTORY_ID)
                    || getInventory.ApiMessage.Equals(InventoryConstants.INVENTORY_NOT_FOUND))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(getInventory)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getInventory)
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
        [Route("update-inventory")]
        public async Task<IActionResult> UpdateInventory([FromBody] CreateUpdateInventoryRequest req)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iInventoryService.UpdateInventory(req);
                if (!result.IsSuccess && result.ApiMessage.Equals(InventoryConstants.UPDATE_INVENTORY_FAILED))
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
        [HttpPut("soft-delete-inventory-by-id/{id}")]
        public IActionResult SoftDeleteInventoryById(string id)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iInventoryService.SoftDeleteInventoryById(id);
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
