using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IProfileService
    {
        ProfileResponse GetProfileById(long id);
        ProfileResponse UpdateProfile(CreateUpdateProfileRequest req);
    }
}
