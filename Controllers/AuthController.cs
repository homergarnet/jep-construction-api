using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace jep_construction_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IAuthService _iAuthService;

        public AuthController(IAuthService iAuthService, IConfiguration configuration)
        {
            _iAuthService = iAuthService;
            _configuration = configuration;
        }

        [HttpGet]
        [Route("auth-test")]
        public IActionResult AuthTest()
        {
            return Ok("Ok");
        }

        [HttpPost]
        [Route("create-account")]
        public IActionResult CreateAccount([FromBody] UserDto userInfo)
        {
            try
            {

                var createAccount = _iAuthService.CreateAccount(userInfo);
                if (!createAccount.IsSuccess && createAccount.ApiMessage.Equals(AuthConstants.EMAIL_ALREADY_EXIST) 
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

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginDto loginInfo)
        {
            try
            {

                var login = _iAuthService.Login(loginInfo);
                if (!login.IsSuccess && login.ApiMessage.Equals(AuthConstants.WRONG_USER_PASSWORD))
                {
                    return new ContentResult
                    {
                        StatusCode = 400,
                        ContentType = "application/json",
                        Content = JsonSerializer.Serialize(login)
                    };
                }

                return new ContentResult
                {
                    StatusCode = 200,
                    ContentType = "application/json",
                    Content = JsonSerializer.Serialize(login)
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