using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using barangay_crime_complaint_api.Models;
using barangay_crime_compliant_api.DTOS;
using barangay_crime_compliant_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace barangay_crime_compliant_api.Controllers
{
    public class AuthController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAuthService _iAuthService;

        public AuthController(IAuthService iAuthService, IConfiguration configuration)
        {
            _iAuthService = iAuthService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("api/create-account")]
        public IActionResult CreateAccount([FromBody] UserDto userInfo)
        {
            try
            {

                var user = _iAuthService.CreateAccount(userInfo);
                if (user == "User Already Exist")
                {
                    return new ContentResult
                    {
                        StatusCode = 500,
                        ContentType = "application/json",
                        Content = user
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = user
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

        [HttpPost]
        [Route("api/create-personal-info")]
        public IActionResult CreatePersonalInfo(

            [FromForm] IFormFile ValidId, [FromForm] IFormFile SelfieId, [FromForm] string Username,
            [FromForm] string Password, [FromForm] string FirstName, [FromForm] string MiddleName,
            [FromForm] string LastName, [FromForm] DateTime BirthDate, [FromForm] string Gender,
            [FromForm] string Phone, [FromForm] string HouseNo, [FromForm] string Street,
            [FromForm] string Village, [FromForm] string UnitFloor, [FromForm] string Building,
            [FromForm] string ProvinceCode, [FromForm] string CityCode, [FromForm] string BrgyCode,
            [FromForm] string ZipCode, [FromForm] DateTime DateCreated, [FromForm] string UserType,
            [FromForm] string ResidencyType, [FromForm] string Email
        )
        {
            try
            {

                if (ValidId == null || ValidId.Length == 0)
                {
                    return new ContentResult
                    {
                        StatusCode = 404,
                        ContentType = "application/json",
                        Content = "No Valid image uploaded"
                    };
                }

                if (SelfieId == null || SelfieId.Length == 0)
                {
                    return new ContentResult
                    {
                        StatusCode = 404,
                        ContentType = "application/json",
                        Content = "No Selfie image uploaded"
                    };
                }


                var user = _iAuthService.CreatePersonalInfo(
                    ValidId, SelfieId, Username, Password, FirstName, MiddleName, LastName, BirthDate,
                    Gender, Phone, HouseNo, Street, Village, UnitFloor, Building, ProvinceCode, CityCode,
                    BrgyCode, ZipCode, DateCreated, UserType, ResidencyType, Email
                );

                if (user == "User Already Exist")
                {
                    return new ContentResult
                    {
                        StatusCode = 500,
                        ContentType = "application/json",
                        Content = user
                    };
                }

                if (user == "Email Already Exist")
                {
                    return new ContentResult
                    {
                        StatusCode = 500,
                        ContentType = "application/json",
                        Content = user
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(true)
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

        [HttpPost]
        [Route("api/login")]
        public IActionResult Login([FromBody] LoginDto loginInfo)
        {
            try
            {

                var loginRes = _iAuthService.Login(loginInfo);
                if (loginRes.Equals("Wrong User or Password"))
                {
                    return new ContentResult
                    {
                        StatusCode = 500,
                        ContentType = "application/json",
                        Content = loginRes
                    };
                }
                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = loginRes
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

        [HttpGet]
        [Route("api/get-user-personal-info")]
        public IActionResult GetUserPersonalInfoList(
            [FromQuery] string keyword, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var getUserPersonalInfoList = _iAuthService.GetUserPersonalInfoList(keyword, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getUserPersonalInfoList)
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

        [HttpGet]
        [Route("api/get-responder-info-list")]
        public IActionResult GetResponderInfoList(
            [FromQuery] string keyword, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {

            try
            {

                var getResponderInfoList = _iAuthService.GetResponderInfoList(keyword, page, pageSize);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getResponderInfoList)
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

        [HttpGet]
        [Route("api/get-user-personal-info-by-id")]
        public IActionResult GetUserPersonalInfoById(
            [FromQuery] long id
        )
        {

            try
            {

                var getUserPersonalInfoList = _iAuthService.GetUserPersonalInfoById(id);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getUserPersonalInfoList)
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
        [Route("api/get-current-user-personal-info")]
        public IActionResult GetUserPersonalInfoById(
        )
        {

            try
            {

                var userId = Convert.ToInt64(User.FindFirst("UserId").Value);

                var getUserPersonalInfo = _iAuthService.GetUserPersonalInfoById(userId);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getUserPersonalInfo)
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
        [Route("api/is-current-responder")]
        public IActionResult IsCurrentResponder(
            [FromQuery] long responderId
        )
        {

            try
            {

                var userId = Convert.ToInt64(User.FindFirst("UserId").Value);

                var getUserPersonalInfo = _iAuthService.IsCurrentResponder(responderId,userId);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(getUserPersonalInfo)
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

        [HttpPut]
        [Route("api/update-password")]
        public IActionResult UpdatePassword(

            [FromBody] UpdatePasswordDto updatePasswordInfo

        )
        {

            try
            {


                var updatePassword = _iAuthService.UpdatePassword(updatePasswordInfo);

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = updatePassword
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