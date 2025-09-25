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
    public class ProfileController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IProfileService _iProfileService;
        private readonly IWebHostEnvironment _env;

        public ProfileController(IConfiguration configuration, IProfileService iProfileService, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _iProfileService = iProfileService;
            _env = env;
        }

        [Authorize]
        [HttpPut]
        [Route("update-profile")]
        public IActionResult UpdateProfile([FromForm] CreateUpdateProfileRequest req)
        {
            try
            {
                if (req.ProfileImage == null || req.ProfileImage.Length == 0)
                {
                    return new ContentResult
                    {
                        StatusCode = 404,
                        ContentType = "application/json",
                        Content = "No Valid image uploaded"
                    };
                }
                var updateProfile = _iProfileService.UpdateProfile(req);
                if (!updateProfile.IsSuccess && updateProfile.ApiMessage.Equals(ProfileConstants.UPDATE_PROFILE_FAILED))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(updateProfile)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(updateProfile)
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
        [Route("get-profile-by-id")]
        public IActionResult GetProfileById([FromQuery] long id = 0)
        {

            try
            {

                var userEmailAdd = User.FindFirst("UserEmailAdd")?.Value;
                var getProfile = _iProfileService.GetProfileById(id);
                if (!getProfile.IsSuccess && getProfile.ApiMessage.Equals(ProfileConstants.INVALID_PROFILE_ID)
                    || getProfile.ApiMessage.Equals(ProfileConstants.PROFILE_NOT_FOUND))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(getProfile)
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getProfile)
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

        //[HttpPost("time-in")]
        //public async Task<IActionResult> TimeIn([FromBody] AttendanceDto dto)
        //{
        //    if (string.IsNullOrEmpty(dto.TimeInOutImage))
        //        return BadRequest("No image provided.");

        //    try
        //    {
        //        // Remove data:image/png;base64, prefix if present
        //        var base64Data = dto.TimeInOutImage.Split(',')[1];
        //        var bytes = Convert.FromBase64String(base64Data);

        //        // Ensure uploads folder exists
        //        var uploadsFolder = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "uploads");
        //        if (!Directory.Exists(uploadsFolder))
        //            Directory.CreateDirectory(uploadsFolder);

        //        // Generate unique file name
        //        var fileName = $"timein_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        //        var filePath = Path.Combine(uploadsFolder, fileName);

        //        await System.IO.File.WriteAllBytesAsync(filePath, bytes);

        //        return Ok(new
        //        {
        //            dto.Location,
        //            dto.TimeIn,
        //            ImageUrl = $"/uploads/{fileName}" // return the saved file path
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Error saving image: {ex.Message}");
        //    }
        //}

    }
}
