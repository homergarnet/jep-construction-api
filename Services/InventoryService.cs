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
    public class InventoryService : IInventoryService
    {

        private readonly IConfiguration configuration;
        private readonly Jep_ConstructionContext db;
        private readonly string _connectionString;
        private string inventoryQuery = string.Empty;

        public InventoryService(IConfiguration configuration, Jep_ConstructionContext db)
        {
            this.configuration = configuration;
            this.db = db;
            _connectionString = configuration.GetConnectionString("Jep_Construction");
        }

        public InventoryResponse CreateInventory(CreateUpdateInventoryRequest req)
        {

            var response = new InventoryResponse
            {
                InventoryList = new List<InventoryDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };


            // user.Id = item.Id;
            var inventoryExist = db.Inventories.Any(z => z.ItemName == req.ItemName);
            if (inventoryExist)
            {
                response.IsSuccess = false;
                response.ApiMessage = InventoryConstants.INVENTORY_IN_USER_ALREADY_EXIST;

            }
            else
            {

                Inventory inventory = new Inventory();
                inventory.UserId = req.UserId;
                inventory.ItemName = req.ItemName?.Trim() ?? "";
                inventory.Category = req.Category?.Trim() ?? "";
                inventory.Quantity = req.Quantity;
                inventory.UnitOfMeasure = req.UnitOfMeasure?.Trim() ?? "";
                inventory.ReOrderLevel = req.ReOrderLevel;
                inventory.ReOrderQuantity = req.ReOrderQuantity;
                inventory.Description = req.Description?.Trim() ?? "";
                inventory.DateTimeCreated = Common.DateTimeNow("Singapore Standard Time");
                db.Inventories.Add(inventory);
                db.SaveChanges();
                response.IsSuccess = true;
                response.ApiMessage = InventoryConstants.CREATE_INVENTORY_SUCCESS;

            }

            return response;

        }

        public InventoryResponse GetInventoryById(long id)
        {
            var response = new InventoryResponse
            {
                InventoryList = new List<InventoryDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            try
            {
                if (id <= 0)
                {
                    response.ApiMessage = InventoryConstants.INVALID_INVENTORY_ID;
                    return response;
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                        SELECT i.Id, i.UserId, i.ItemName, i.Category, i.Quantity, i.UnitOfMeasure, i.ReOrderLevel, i.ReOrderQuantity, i.Description,
                        (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[Inventory] i
                        INNER JOIN [dbo].[User] u ON u.Id = i.UserId
                        WHERE i.Id = @Id AND i.IsEnabled = 1";

                    var employee = connection.QueryFirstOrDefault<InventoryDto>(query, new { Id = id });

                    if (employee != null)
                    {
                        response.InventoryList.Add(employee);
                        response.TotalRecords = 1;
                        response.IsSuccess = true;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.ApiMessage = InventoryConstants.INVENTORY_NOT_FOUND;
                    }
                }
            }
            catch (Exception ex)
            {
                response.ApiMessage = ex.Message;
            }

            return response;
        }

        public InventoryResponse GetInventoryList(string keyword, long? userId, int page, int pageSize)
        {

            if (!string.IsNullOrWhiteSpace(keyword) && keyword.Equals("not/a"))
            {
                // no keyword filter → return all employees (paged)
                keyword = "";
            }

            var response = new InventoryResponse
            {
                InventoryList = new List<InventoryDto>(), // or UserList depending on your model
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

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
                        FROM [dbo].[Inventory] i
                        INNER JOIN [dbo].[User] u ON u.Id = i.UserId
                        WHERE (@Keyword = '' OR i.ItemName LIKE '%' + @Keyword + '%')
                        AND i.UserId = @UserId AND i.IsEnabled = 1";

                    var totalCount = connection.ExecuteScalar<long>(countQuery, new
                    {
                        Keyword = keyword,
                        UserId = userId
                    });
                    // Paginated Data
                    dataQuery = @"
                        SELECT i.Id, i.UserId, i.ItemName, i.Category, i.Quantity, i.UnitOfMeasure, i.ReOrderLevel, i.ReOrderQuantity, i.Description,
                        (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[Inventory] i
                        INNER JOIN [dbo].[User] u ON u.Id = i.UserId
                        WHERE (@Keyword = '' OR i.ItemName LIKE '%' + @Keyword + '%')
                        AND i.UserId = @UserId AND i.IsEnabled = 1
                        ORDER BY i.Id DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";
                    var data = connection.Query<InventoryDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize,
                        UserId = userId
                    }).ToList();

                    // Set response
                    response.InventoryList = data;
                    response.TotalRecords = totalCount;
                }
                else
                {
                    // Total Count
                    var countQuery = @"
                        SELECT COUNT(*)
                        FROM [dbo].[Inventory] i
                        INNER JOIN [dbo].[User] u ON u.Id = i.UserId
                        WHERE (@Keyword = '' OR i.ItemName LIKE '%' + @Keyword + '%') AND i.IsEnabled = 1";

                    var totalCount = connection.ExecuteScalar<long>(countQuery, new { Keyword = keyword });
                    // Paginated Data
                    dataQuery = @"
                        SELECT i.Id, i.UserId, i.ItemName, i.Category, i.Quantity, i.UnitOfMeasure, i.ReOrderLevel, i.ReOrderQuantity, i.Description,
                        (COALESCE(u.Firstname, '') + ' ' + COALESCE(u.Lastname, '')) AS ClientName
                        FROM [dbo].[Inventory] i
                        INNER JOIN [dbo].[User] u ON u.Id = i.UserId
                        WHERE (@Keyword = '' OR i.ItemName LIKE '%' + @Keyword + '%') AND i.IsEnabled = 1
                        ORDER BY i.Id DESC
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY";
                    var data = connection.Query<InventoryDto>(dataQuery, new
                    {
                        Keyword = keyword,
                        Offset = (page - 1) * pageSize,
                        PageSize = pageSize
                    }).ToList();

                    // Set response
                    response.InventoryList = data;
                    response.TotalRecords = totalCount;
                }

                response.IsSuccess = true;

            }

            return response;
        }

        public InventoryResponse SoftDeleteInventoryById(string id)
        {
            var response = new InventoryResponse
            {
                InventoryList = new List<InventoryDto>(),
                TotalRecords = 0L,
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using (var connection = new SqlConnection(_connectionString))
            {

                connection.Open();
                var sql = @"UPDATE [dbo].[Inventory] 
                            SET IsEnabled = 0
                            WHERE Id = @Id";
                int rowsAffected = connection.Execute(sql, new { Id = id });
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.ApiMessage = InventoryConstants.SOFT_DELETE_INVENTORY_SUCCESS;
                }
                else
                {
                    response.IsSuccess = false;
                    response.ApiMessage = InventoryConstants.SOFT_DELETE_INVENTORY_FAILED;
                }

                return response;

            }
        }

        public InventoryResponse UpdateInventory(CreateUpdateInventoryRequest req)
        {
            var response = new InventoryResponse
            {
                InventoryList = new List<InventoryDto>(),
                IsSuccess = false,
                ApiMessage = string.Empty
            };

            using var connection = new SqlConnection(_connectionString);
            var dateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"));
            inventoryQuery = @"UPDATE [dbo].[Inventory] 
                            SET ItemName = @ItemName, Category = @Category, Quantity = @Quantity, UnitOfMeasure = @UnitOfMeasure,
                            ReOrderLevel = @ReOrderLevel, ReOrderQuantity = @ReOrderQuantity, Description = @Description, 
                            DateTimeUpdated = @DateTimeUpdated
                            WHERE Id = @Id
                            ";

            var rowsInserted = connection.Execute(inventoryQuery, new
            {
                Id = req.Id,
                ItemName = req.ItemName,
                Category = req.Category,
                Quantity = req.Quantity,
                UnitOfMeasure = req.UnitOfMeasure,
                ReOrderLevel = req.ReOrderLevel,
                ReOrderQuantity = req.ReOrderQuantity,
                Description = req.Description,
                DateTimeUpdated = dateTimeNow,
            });

            if (rowsInserted > 0)
            {

                response.IsSuccess = true;
                response.ApiMessage = InventoryConstants.UPDATE_INVENTORY_SUCCESS;

            }
            else
            {
                response.IsSuccess = false;
                response.ApiMessage = InventoryConstants.UPDATE_INVENTORY_FAILED;
            }

            return response;
        }
    }
}
