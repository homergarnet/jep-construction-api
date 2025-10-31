using Dapper;
using jep_construction_api.Constants;
using jep_construction_api.DTOS;
using jep_construction_api.Library;
using jep_construction_api.Models;
using jep_construction_api.Request;
using jep_construction_api.Response;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace jep_construction_api.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private string reviewQuery = string.Empty;

        public ReviewService(IConfiguration configuration, Jep_ConstructionContext db)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
        }

        public ReviewResponse CreateReview(CreateUpdateReviewRequest req)
        {
            var response = new ReviewResponse
            {
                ReviewList = new List<ReviewDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };
            // user.Id = item.Id;
            var isExistPMId = db.Reviews.Any(z => z.ProjectManagementId == req.ProjectManagementId);
            if (isExistPMId)
            {
                response.IsSuccess = false;
                response.ApiMessage = ReviewConstants.REVIEW_ALREADY_EXIST;
                return response;
            }
            else
            {
                Review review = new Review();
                review.UserId = req.UserId;
                review.ProjectManagementId = req.ProjectManagementId;
                review.Rate = req.Rate;
                review.ReviewDescription = req.ReviewDescription?.Trim() ?? "";
                review.DateTimeCreated = Common.DateTimeNow("Singapore Standard Time");
                db.Reviews.Add(review);
                db.SaveChanges();
                response.IsSuccess = true;
                response.ApiMessage = ReviewConstants.CREATE_REVIEW_SUCCESS;
                return response;
            }

        }

        public ReviewResponse GetReviewById(long id)
        {
            var response = new ReviewResponse
            {
                ReviewList = new List<ReviewDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };


            if (id <= 0)
            {
                response.ApiMessage = ReviewConstants.INVALID_REVIEW_ID;
                return response;
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var query = @"
                        SELECT r.Id, r.Rate, r.ReviewDescription, r.DateTimeCreated, pm.ProjectName, u.Email, u.MobileNumber, 
                        (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[Review] r
                        INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = r.ProjectManagementId
                        INNER JOIN [dbo].[User] u ON u.Id = r.UserId
                        AND r.Id = @Id AND r.IsEnabled = 1";

                var review = connection.QueryFirstOrDefault<ReviewDto>(query, new { Id = id });

                if (review != null)
                {
                    response.ReviewList.Add(review);
                    response.TotalRecords = 1;
                    response.IsSuccess = true;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = ReviewConstants.REVIEW_NOT_FOUND;
                }
            }

            return response;

        }

        public ReviewResponse GetReviewList(string keyword, long? userId, bool? isApprove, int page, int pageSize)
        {
            var response = new ReviewResponse
            {
                ReviewList = new List<ReviewDto>(), // or UserList depending on your model
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            try
            {
                keyword = keyword ?? string.Empty;

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();


                    var dataQuery = "";
                    // if not admin
                    if (userId != 0)
                    {

                        // Total Count
                        var countQuery = @"
                        SELECT COUNT(*)
                        FROM [dbo].[Review] r
                        INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = r.ProjectManagementId
                        INNER JOIN [dbo].[User] u ON u.Id = r.UserId
                        WHERE (@Keyword = '' OR r.ReviewDescription LIKE '%' + @Keyword + '%') 
                        AND pm.UserId = @UserId AND r.IsEnabled = 1";
                        var totalCount = connection.ExecuteScalar<long>(countQuery, new
                        {
                            Keyword = keyword,
                            UserId = userId
                        });
                        // Paginated Data
                        dataQuery = @"
                        SELECT r.Id, r.Rate, r.ReviewDescription, r.DateTimeCreated, pm.ProjectName, u.Email, u.MobileNumber, 
                        (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[Review] r
                        INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = r.ProjectManagementId
                        INNER JOIN [dbo].[User] u ON u.Id = r.UserId
                        WHERE (@Keyword = '' OR r.ReviewDescription LIKE '%' + @Keyword + '%') 
                        AND pm.UserId = @UserId AND r.IsEnabled = 1
                        ORDER BY r.Id DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";

                        var data = connection.Query<ReviewDto>(dataQuery, new
                        {
                            Keyword = keyword,
                            Offset = (page - 1) * pageSize,
                            PageSize = pageSize,
                            UserId = userId   // ✅ added
                        }).ToList();

                        response.ReviewList = data;
                        response.TotalRecords = totalCount;

                    }
                    else if(isApprove != null)
                    {
                        // Total Count
                        var countQuery = @"
                        SELECT COUNT(*)
                        FROM [dbo].[Review] r
                        INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = r.ProjectManagementId
                        LEFT JOIN [dbo].[User] u ON u.Id = r.UserId
                        WHERE (@Keyword = '' OR r.ReviewDescription LIKE '%' + @Keyword + '%') 
                        AND r.IsEnabled = 1 AND r.IsApprove = @IsApprove";
                        var totalCount = connection.ExecuteScalar<long>(countQuery, new
                        {
                            Keyword = keyword,
                            IsApprove = isApprove
                        });
                        // Paginated Data
                        dataQuery = @"
                        SELECT r.Id, r.Rate, r.ReviewDescription, r.DateTimeCreated, pm.ProjectName, u.Email, u.MobileNumber, 
                        (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[Review] r
                        INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = r.ProjectManagementId
                        LEFT JOIN [dbo].[User] u ON u.Id = r.UserId
                        WHERE (@Keyword = '' OR r.ReviewDescription LIKE '%' + @Keyword + '%') 
                        AND r.IsEnabled = 1 AND r.IsApprove = @IsApprove
                        ORDER BY r.Id DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";

                        var data = connection.Query<ReviewDto>(dataQuery, new
                        {
                            Keyword = keyword,
                            Offset = (page - 1) * pageSize,
                            PageSize = pageSize,
                            IsApprove = isApprove

                        }).ToList();

                        response.ReviewList = data;
                        response.TotalRecords = totalCount;
                    }
                    // if admin
                    else
                    {
                        // Total Count
                        var countQuery = @"
                        SELECT COUNT(*)
                        FROM [dbo].[Review] r
                        INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = r.ProjectManagementId
                        INNER JOIN [dbo].[User] u ON u.Id = r.UserId
                        WHERE (@Keyword = '' OR r.ReviewDescription LIKE '%' + @Keyword + '%') AND r.IsEnabled = 1";
                        var totalCount = connection.ExecuteScalar<long>(countQuery, new
                        {
                            Keyword = keyword,

                        });
                        // Paginated Data
                        dataQuery = @"
                        SELECT r.Id, r.Rate, r.ReviewDescription, r.DateTimeCreated, pm.ProjectName, u.Email, u.MobileNumber, 
                        (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[Review] r
                        INNER JOIN [dbo].[ProjectManagement] pm ON pm.Id = r.ProjectManagementId
                        INNER JOIN [dbo].[User] u ON u.Id = r.UserId
                        WHERE (@Keyword = '' OR r.ReviewDescription LIKE '%' + @Keyword + '%') AND r.IsEnabled = 1
                        ORDER BY r.Id DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";
                        var data = connection.Query<ReviewDto>(dataQuery, new
                        {
                            Keyword = keyword,
                            Offset = (page - 1) * pageSize,
                            PageSize = pageSize
                        }).ToList();

                        // Set response
                        response.ReviewList = data;
                        response.TotalRecords = totalCount;
                    }


                    response.IsSuccess = true;

                }
            }
            catch (Exception ex)
            {
                response.ApiMessage = ex.Message;
            }

            return response;

        }

        public ReviewResponse SoftDeleteReviewById(string id)
        {
            var response = new ReviewResponse
            {
                ReviewList = new List<ReviewDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[Review] 
                            SET IsEnabled = 0
                            WHERE Id = @Id";
                int rowsAffected = connection.Execute(sql, new { Id = id });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = ReviewConstants.SOFT_DELETE_REVIEW_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = ReviewConstants.SOFT_DELETE_REVIEW_FAILED;
                }

                return response;

            }
        }

        public ReviewResponse UpdateApproveReviewPost(UpdateApproveReviewPostRequest req)
        {
            var response = new ReviewResponse
            {
                ReviewList = new List<ReviewDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using var connection = new SqlConnection(_connectionString);
            var dateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
            reviewQuery = @"UPDATE [dbo].[Review] 
                            SET IsApprove = @IsApprove, DateTimeUpdated = @DateTimeUpdated
                            WHERE Id = @Id
                            ";

            var rowsInserted = connection.Execute(reviewQuery, new
            {
                Id = req.Id,
                IsApprove = req.IsApprove,
                DateTimeUpdated = dateTimeNow,
            });

            if (rowsInserted > 0)
            {

                response.IsSuccess = true;
                response.ApiMessage = ReviewConstants.UPDATE_APPROVE_REVIEW_SUCCESS;

            }
            else
            {
                response.IsSuccess = false;
                response.ApiMessage = ReviewConstants.UPDATE_APPROVE_REVIEW_FAILED;
            }

            return response;
        }

        public ReviewResponse UpdateReview(CreateUpdateReviewRequest req)
        {
            var response = new ReviewResponse
            {
                ReviewList = new List<ReviewDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using var connection = new SqlConnection(_connectionString);
            var dateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
            reviewQuery = @"UPDATE [dbo].[Review] 
                            SET Rate = @Rate, ReviewDescription = @ReviewDescription, DateTimeUpdated = @DateTimeUpdated
                            WHERE Id = @Id
                            ";

            var rowsInserted = connection.Execute(reviewQuery, new
            {
                Id = req.Id,
                Rate = req.Rate,
                ReviewDescription = req.ReviewDescription,
                DateTimeUpdated = dateTimeNow,
            });

            if (rowsInserted > 0)
            {

                response.IsSuccess = true;
                response.ApiMessage = ReviewConstants.UPDATE_REVIEW_SUCCESS;

            }
            else
            {
                response.IsSuccess = false;
                response.ApiMessage = ReviewConstants.UPDATE_REVIEW_FAILED;
            }

            return response;
        }
    }
}
