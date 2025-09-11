

using jep_construction_api.DTOS;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{

    public interface IAuthService
    {

        AuthResponse CreateAccount(UserDto userReq);
        AuthResponse Login(LoginDto loginInfo);
        //string CreatePersonalInfo(
        //    IFormFile ValidId, IFormFile SelfieId, string Username, string Password, string FirstName, 
        //    string MiddleName, string LastName, DateTime BirthDate, string Gender, string Phone, 
        //    string HouseNo, string Street, string Village, string UnitFloor, string Building, 
        //    string ProvinceCode, string CityCode, string BrgyCode, string ZipCode, DateTime DateCreated, 
        //    string UserType, string ResidencyType, string Email
        //);
        //string Login(LoginDto loginInfo);
        //List<UserDto> GetUserPersonalInfoList( string keyword, int page, int pageSize);
        //List<UserDto> GetResponderInfoList( string keyword, int page, int pageSize);
        //UserDto GetUserPersonalInfoById(long id);
        //bool IsCurrentResponder(long responderId, long id);
        //string UpdatePassword(UpdatePasswordDto updatePasswordInfo);
    }
}