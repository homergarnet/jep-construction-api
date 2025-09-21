using jep_construction_api.Request;
using jep_construction_api.Response;

namespace jep_construction_api.Services
{
    public interface IReviewService
    {
        ReviewResponse CreateReview(CreateUpdateReviewRequest req);
        ReviewResponse GetReviewById(long id);
        ReviewResponse GetReviewList(string keyword, long? userId, int page, int pageSize);
        ReviewResponse SoftDeleteReviewById(string id);
        ReviewResponse UpdateReview(CreateUpdateReviewRequest req);
    }
}
