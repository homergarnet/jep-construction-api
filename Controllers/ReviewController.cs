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
    public class ReviewController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IReviewService _iReviewService;

        public ReviewController(IConfiguration configuration, IReviewService iReviewService)
        {
            _configuration = configuration;
            _iReviewService = iReviewService;
        }

        [Authorize]
        [HttpPost]
        [Route("create-review")]
        public IActionResult CreateReview([FromBody] CreateUpdateReviewRequest req)
        {
            try
            {

                var createReview = _iReviewService.CreateReview(req);
                if(!createReview.IsSuccess && createReview.ApiMessage.Equals(ReviewConstants.REVIEW_ALREADY_EXIST))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(createReview)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(createReview)
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
        [Route("get-review-list")]
        public IActionResult GetReviewList(
            [FromQuery] string? keyword = "", [FromQuery] long? userId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getReviewList = _iReviewService.GetReviewList(keyword ?? "", userId, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getReviewList)
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
        [Route("get-review-by-id")]
        public IActionResult GetReviewById([FromQuery] long id = 0)
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getReview = _iReviewService.GetReviewById(id);
                if (!getReview.IsSuccess && getReview.ApiMessage.Equals(ReviewConstants.INVALID_REVIEW_ID)
                    || getReview.ApiMessage.Equals(ReviewConstants.REVIEW_NOT_FOUND))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(getReview)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getReview)
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
        [Route("update-review")]
        public async Task<IActionResult> UpdateReview([FromBody] CreateUpdateReviewRequest req)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iReviewService.UpdateReview(req);
                if (!result.IsSuccess && result.ApiMessage.Equals(ReviewConstants.UPDATE_REVIEW_FAILED))
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
        [HttpPut("soft-delete-review-by-id/{id}")]
        public IActionResult SoftDeleteReviewById(string id)
        {
            try
            {
                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var result = _iReviewService.SoftDeleteReviewById(id);
                if (!result.IsSuccess && result.ApiMessage.Equals(ReviewConstants.SOFT_DELETE_REVIEW_FAILED))
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
