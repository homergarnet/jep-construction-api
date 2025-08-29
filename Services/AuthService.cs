using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using barangay_crime_complaint_api.Models;
using barangay_crime_compliant_api.DTOS;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace barangay_crime_compliant_api.Services
{
    public class AuthService : IAuthService
    {

        private readonly IConfiguration configuration;
        private readonly Thesis_CrimeContext db;
        public AuthService(IConfiguration configuration, Thesis_CrimeContext db)
        {
            this.configuration = configuration;
            this.db = db;
        }

        public string CreateAccount(UserDto item)
        {

            User user = new User();
            // user.Id = item.Id;
            var userExist = db.Users.Any(z => z.Username == item.Username);
            if (userExist)
            {
                return "User Already Exist";
            }
            else
            {

                user.Username = item.Username;
                user.Password = BCrypt.Net.BCrypt.HashPassword(item.Password);
                user.FirstName = item.FirstName;
                user.MiddleName = item.MiddleName;
                user.LastName = item.LastName;
                user.BirthDate = item.BirthDate;
                user.Gender = item.Gender;
                user.Phone = item.Phone;
                user.HouseNo = item.HouseNo;
                user.Street = item.Street;
                user.Village = item.Village;
                user.UnitFloor = item.UnitFloor;
                user.Building = item.Building;
                user.ProvinceCode = item.ProvinceCode;
                user.CityCode = item.CityCode;
                user.BrgyCode = item.BrgyCode;
                user.ZipCode = item.ZipCode;
                user.DateCreated = DateTime.Now;
                user.DateUpdated = DateTime.Now;
                user.UserType = item.UserType;
                db.Users.Add(user);
                db.SaveChanges();

                return "Successfully Created Account";
            }



        }

        public string CreatePersonalInfo(
            IFormFile ValidId, IFormFile SelfieId, string Username, string Password, string FirstName,
            string MiddleName, string LastName, DateTime BirthDate, string Gender, string Phone,
            string HouseNo, string Street, string Village, string UnitFloor, string Building,
            string ProvinceCode, string CityCode, string BrgyCode, string ZipCode, DateTime DateCreated,
            string UserType, string ResidencyType, string Email
        )
        {

            User user = new User();
            // user.Id = item.Id;
            // user.Id = item.Id;
            var userExist = db.Users.Any(z => z.Username == Username);
            var emailExist = db.Users.Any(z => z.Email == Email);
            if (userExist)
            {
                return "User Already Exist";
            }
            if (emailExist)
            {
                return "Email Already Exist";
            }
            user.Username = Username;
            user.Password = BCrypt.Net.BCrypt.HashPassword(Password);
            user.FirstName = FirstName;
            user.MiddleName = MiddleName;
            user.LastName = LastName;
            user.BirthDate = BirthDate;
            user.Gender = Gender;
            user.Phone = Phone;
            user.HouseNo = HouseNo;
            user.Street = Street;
            user.Village = Village;
            user.UnitFloor = UnitFloor;
            user.Building = Building;
            user.ProvinceCode = ProvinceCode;
            user.CityCode = CityCode;
            user.BrgyCode = BrgyCode;
            user.ZipCode = ZipCode;
            user.DateCreated = DateTime.Now;
            user.DateUpdated = DateTime.Now;
            user.UserType = UserType;
            user.ResidencyType = ResidencyType;
            user.Email = Email;

            // Define a target directory to save the uploaded file
            var targetDirectory = "uploads"; // Change this to your desired directory

            // Ensure the target directory exists
            Directory.CreateDirectory(targetDirectory);

            // Generate a unique file name to avoid overwriting
            var validId = Path.Combine(targetDirectory, Guid.NewGuid().ToString() + "_" + ValidId.FileName);
            var selfie = Path.Combine(targetDirectory, Guid.NewGuid().ToString() + "_" + SelfieId.FileName);

            // Save the file to the server
            using (var fileStream = new FileStream(validId, FileMode.Create))
            {

                ValidId.CopyTo(fileStream);

            }

            using (var fileStream = new FileStream(selfie, FileMode.Create))
            {

                ValidId.CopyTo(fileStream);

            }

            using (var memoryStream = new MemoryStream())
            {
                ValidId.CopyTo(memoryStream);
                user.ValidId = validId;
            }
            using (var memoryStream = new MemoryStream())
            {

                SelfieId.CopyTo(memoryStream);
                user.Selfie = selfie;

            }


            db.Users.Add(user);
            db.SaveChanges();

            return "Create Personal Info Created Successfully";

        }

        public string Login(LoginDto loginDto)
        {

            var user = db.Users.Where(z => z.Username == loginDto.Username).FirstOrDefault();
            bool verified = false;
            string password = loginDto.Password.Trim();

            if (user != null)
            {
                verified = BCrypt.Net.BCrypt.Verify(password, user.Password);
                if (verified)
                {

                    // User Claims
                    var claims = new List<Claim>
                    {
                        new Claim("UserId", user.Id.ToString()),
                        new Claim("Username", user.Username.ToString()),
                        new Claim("FirstName", user.FirstName.ToString()),
                        new Claim("MiddleName", user.MiddleName.ToString()),
                        new Claim("LastName", user.LastName.ToString()),
                        new Claim("UserType", user.UserType.ToString()),

                    };

                    // Encrypt credentials
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var auth = new JwtSecurityToken(configuration["Jwt:Issuer"],
                        configuration["Jwt:Issuer"],
                        claims,
                        expires: DateTime.Now.AddHours(8766), // Set to 1 year
                        signingCredentials: credentials);

                    // Generate JWT
                    var token = new JwtSecurityTokenHandler().WriteToken(auth);

                    if (loginDto.UserType.Equals("admin") || loginDto.UserType.Equals("police"))
                    {
                        if (user.UserType.Equals(loginDto.UserType) || user.UserType.Equals("police"))
                        {
                            return token;
                        }
                        else
                        {
                            return "Wrong User or Password";
                        }

                    }
                    else if (loginDto.UserType.Equals("barangay"))
                    {

                        if (user.UserType.Equals(loginDto.UserType))
                        {
                            return token;
                        }
                        else
                        {
                            return "Wrong User or Password";
                        }

                    }

                    else if (loginDto.UserType.Equals("compliant"))
                    {

                        if (user.UserType.Equals(loginDto.UserType))
                        {
                            return token;
                        }
                        else
                        {
                            return "Wrong User or Password";
                        }

                    }

                }

                //Not verified
                else
                {
                    return "Wrong User or Password";
                }

            }

            return "Wrong User or Password";

        }

        public List<UserDto> GetUserPersonalInfoList(string keyword, int page, int pageSize)
        {
            IQueryable<User> query = db.Users.Where(z => !z.UserType.Equals("admin"));

            var provinceQuery = db.PhProvinces;
            var cityQuery = db.PhCities;
            var barangayQuery = db.PhBrgies;

            List<UserDto> getUserPersonalInfoList = new List<UserDto>();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(
                    z => z.FirstName.Contains(keyword) || z.MiddleName.Contains(keyword) ||
                    z.LastName.Contains(keyword) || z.ProvinceCodeNavigation.ProvDesc.Contains(keyword) ||
                    z.CityCodeNavigation.CityDescription.Contains(keyword) || z.BrgyCodeNavigation.BrgyCode.Contains(keyword) ||
                    z.BrgyCodeNavigation.BrgyName.Contains(keyword) || z.Phone.Contains(keyword)
                );
            }

            var userPersonalInfoRes = query
            .Include(z => z.ProvinceCodeNavigation)
            .Include(z => z.CityCodeNavigation)
            .Include(z => z.BrgyCodeNavigation)
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();

            foreach (var userPersonalInfo in userPersonalInfoRes)
            {

                var userPersonalInfoDto = new UserDto();
                userPersonalInfoDto.Id = userPersonalInfo.Id;
                userPersonalInfoDto.FirstName = userPersonalInfo.FirstName;
                userPersonalInfoDto.MiddleName = userPersonalInfo.MiddleName;
                userPersonalInfoDto.LastName = userPersonalInfo.LastName;
                userPersonalInfoDto.ProvinceName = userPersonalInfo.ProvinceCodeNavigation.ProvDesc;
                userPersonalInfoDto.CityName = userPersonalInfo.CityCodeNavigation.CityDescription;
                userPersonalInfoDto.BarangayName = userPersonalInfo.BrgyCodeNavigation.BrgyName;
                userPersonalInfoDto.Phone = userPersonalInfo.Phone;
                userPersonalInfoDto.ValidIdImage = userPersonalInfo.ValidId;
                userPersonalInfoDto.SelfieIdImage = userPersonalInfo.Selfie;
                userPersonalInfoDto.HouseNo = userPersonalInfo.HouseNo;
                userPersonalInfoDto.Street = userPersonalInfo.Street;
                userPersonalInfoDto.Village = userPersonalInfo.Village;
                userPersonalInfoDto.UnitFloor = userPersonalInfo.UnitFloor;
                getUserPersonalInfoList.Add(userPersonalInfoDto);

            }

            return getUserPersonalInfoList;
        }

        public List<UserDto> GetResponderInfoList(string keyword, int page, int pageSize)
        {
            IQueryable<User> query = db.Users.Where(z => z.UserType.Equals("police"));

            var provinceQuery = db.PhProvinces;
            var cityQuery = db.PhCities;
            var barangayQuery = db.PhBrgies;

            List<UserDto> getResponderInfoList = new List<UserDto>();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(
                    z => z.FirstName.Contains(keyword) || z.MiddleName.Contains(keyword) ||
                    z.LastName.Contains(keyword) || z.ProvinceCodeNavigation.ProvDesc.Contains(keyword) ||
                    z.CityCodeNavigation.CityDescription.Contains(keyword) || z.BrgyCodeNavigation.BrgyCode.Contains(keyword) ||
                    z.BrgyCodeNavigation.BrgyName.Contains(keyword) || z.Phone.Contains(keyword)
                );
            }

            var userPersonalInfoRes = query
            .Include(z => z.ProvinceCodeNavigation)
            .Include(z => z.CityCodeNavigation)
            .Include(z => z.BrgyCodeNavigation)
            .Skip((page - 1) * pageSize).Take(pageSize).ToList();

            foreach (var userPersonalInfo in userPersonalInfoRes)
            {

                var userPersonalInfoDto = new UserDto();
                userPersonalInfoDto.id = userPersonalInfo.Id;
                userPersonalInfoDto.name = userPersonalInfo.FirstName + " " + userPersonalInfo.MiddleName + " " + userPersonalInfo.LastName;
                getResponderInfoList.Add(userPersonalInfoDto);

            }

            return getResponderInfoList;
        }

        public UserDto GetUserPersonalInfoById(long id)
        {

            IQueryable<User> query = db.Users;
            var provinceQuery = db.PhProvinces;
            var cityQuery = db.PhCities;
            var barangayQuery = db.PhBrgies;

            var userPersonalInfo = new UserDto();
            var hasUserPersonalInfo = db.Users.Any(z => z.Id == id);
            if (hasUserPersonalInfo)
            {

                var userPersonalInfoRes = query
                .Include(z => z.ProvinceCodeNavigation)
                .Include(z => z.CityCodeNavigation)
                .Include(z => z.BrgyCodeNavigation)
                .Where(z => z.Id == id).FirstOrDefault();
                userPersonalInfo.Id = userPersonalInfoRes.Id;
                userPersonalInfo.FirstName = userPersonalInfoRes.FirstName;
                userPersonalInfo.MiddleName = userPersonalInfoRes.MiddleName;
                userPersonalInfo.LastName = userPersonalInfoRes.LastName;
                userPersonalInfo.ProvinceName = userPersonalInfoRes.ProvinceCodeNavigation.ProvDesc;
                userPersonalInfo.CityName = userPersonalInfoRes.CityCodeNavigation.CityDescription;
                userPersonalInfo.BarangayName = userPersonalInfoRes.BrgyCodeNavigation.BrgyName;
                userPersonalInfo.Phone = userPersonalInfoRes.Phone;
                userPersonalInfo.ValidIdImage = userPersonalInfoRes.ValidId;
                userPersonalInfo.SelfieIdImage = userPersonalInfoRes.Selfie;
                userPersonalInfo.UserType = userPersonalInfoRes.UserType;
                userPersonalInfo.ProvinceCode = userPersonalInfoRes.ProvinceCode;
                userPersonalInfo.CityCode = userPersonalInfoRes.CityCode;
                userPersonalInfo.BrgyCode = userPersonalInfoRes.BrgyCode;

                DateTime birthDate = userPersonalInfoRes.BirthDate.Value;
                string birthDateFormatted = birthDate.ToString("yyyy-MM-dd");
                userPersonalInfo.BirthDateStr = birthDateFormatted;
                userPersonalInfo.Gender = userPersonalInfoRes.Gender;
                userPersonalInfo.HouseNo = userPersonalInfoRes.HouseNo;
                userPersonalInfo.Street = userPersonalInfoRes.Street;
                userPersonalInfo.Village = userPersonalInfoRes.Village;
                userPersonalInfo.UnitFloor = userPersonalInfoRes.UnitFloor;
                userPersonalInfo.Building = userPersonalInfoRes.Building;
                userPersonalInfo.SelfieIdImage = userPersonalInfoRes.Selfie;
            }

            return userPersonalInfo;
        }

        public bool IsCurrentResponder(long responderId, long id)
        {

            IQueryable<User> query = db.Users;
            var hasUserPersonalInfo = db.Users.Any(z => z.Id == id && responderId == id);
            if (hasUserPersonalInfo)
            {

                return true;
            }

            return false;
        }

        public string UpdatePassword(UpdatePasswordDto updatePasswordInfo)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "")),

            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(updatePasswordInfo.Token, tokenValidationParameters, out validatedToken);

            // Check if the token has expired
            if (validatedToken.ValidTo < DateTime.UtcNow)
            {
                return "Token has expired.";
            }
            else
            {
                var hasEmail = db.Users.Any(z => z.Email.Equals(updatePasswordInfo.Email) && z.ForgotPasswordToken.Equals(updatePasswordInfo.Token));
                if (hasEmail)
                {

                    var email = db.Users.Where(z => z.Email.Equals(updatePasswordInfo.Email) && z.ForgotPasswordToken.Equals(updatePasswordInfo.Token)).First();
                    email.Password = BCrypt.Net.BCrypt.HashPassword(updatePasswordInfo.NewPassword);
                    email.ForgotPasswordToken = "";
                    db.SaveChanges();

                }

                return "Password has been updated";
            }

        }

    }
}