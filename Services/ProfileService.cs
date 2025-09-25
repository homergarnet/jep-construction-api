using Dapper;
using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Models;
using jep_construction_api.Request;
using jep_construction_api.Response;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace jep_construction_api.Services
{
    public class ProfileService : IProfileService
    {

        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private string profileQuery = string.Empty;


        public ProfileService(IConfiguration configuration, Jep_ConstructionContext db)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
        }

        public ProfileResponse GetProfileById(long id)
        {
            var response = new ProfileResponse
            {
                ProfileList = new List<ProfileDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };


            if (id <= 0)
            {
                response.ApiMessage = ProfileConstants.INVALID_PROFILE_ID;
                return response;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"SELECT * FROM [dbo].[User] WHERE Id = @Id";

                var profile = connection.QueryFirstOrDefault<ProfileDto>(query, new { Id = id });

                if (profile != null)
                {
                    response.ProfileList.Add(profile);
                    response.TotalRecords = 1;
                    response.IsSuccess = true;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = ProfileConstants.PROFILE_NOT_FOUND;
                }
            }

            return response;
        }

        public ProfileResponse UpdateProfile(CreateUpdateProfileRequest req)
        {
            var profileImagePath = "";
            // Define a target directory to save the uploaded file
            var targetDirectory = "uploads"; // Change this to your desired directory

            // Ensure the target directory exists
            Directory.CreateDirectory(targetDirectory);
            // Generate a unique file name to avoid overwriting
            var profileImage = Path.Combine(targetDirectory, Guid.NewGuid().ToString() + "_" + req.ProfileImage.FileName);
            // Save the file to the server
            using (var fileStream = new FileStream(profileImage, FileMode.Create))
            {

                req.ProfileImage.CopyTo(fileStream);

            }
            using (var memoryStream = new MemoryStream())
            {
                req.ProfileImage.CopyTo(memoryStream);
                profileImagePath = profileImage;
            }
            var response = new ProfileResponse
            {
                ProfileList = new List<ProfileDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using var connection = new SqlConnection(_connectionString);
            var dateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
            profileQuery = @"UPDATE [dbo].[User] 
                            SET Firstname = @Firstname, Lastname = @Lastname, MobileNumber = @MobileNumber,
                            DateOfBirth = @DateOfBirth, ProfileImage = @ProfileImage, Position = @Position,
                            DateTimeUpdated = @DateTimeUpdated
                            WHERE Id = @Id
                            ";

            var rowsInserted = connection.Execute(profileQuery, new
            {
                Id = req.Id,
                FirstName = req.Firstname,
                LastName = req.Lastname,
                MobileNumber = req.MobileNumber,
                DateOfBirth = req.DateOfBirth,
                ProfileImage = profileImagePath,
                Position = req.Position,
                DateTimeUpdated = dateTimeNow,
            });

            if (rowsInserted > 0)
            {

                response.IsSuccess = true;
                response.ApiMessage = ProfileConstants.UPDATE_PROFILE_SUCCESS;

            }
            else
            {
                response.IsSuccess = false;
                response.ApiMessage = ProfileConstants.UPDATE_PROFILE_FAILED;
            }

            return response;
        }
    }
}
