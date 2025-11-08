using Microsoft.AspNetCore.Mvc;
using RAILWAY_BACKEND.Connection;
using RAILWAY_BACKEND.Models;
using Npgsql;
using System.Collections.Generic;

namespace RAILWAY_BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly DatabaseConnection _dbConnection;

        public SettingsController(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        [HttpGet("hall-types/{adminId}")]
        public async Task<IActionResult> GetHallTypes(string adminId)
        {
            try
            {
                using var connection = _dbConnection.GetConnection();
                await connection.OpenAsync();

                string query = @"
                    SELECT type1, type1_amount, type2, type2_amount, 
                           type3, type3_amount, type4, type4_amount,
                           advance_payment_enabled, default_advance_percentage,
                           hall_name
                    FROM settings
                    WHERE admin_id = @adminId";

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@adminId", adminId);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var types = new List<object>();

                    // Check and add type1
                    if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                    {
                        types.Add(new
                        {
                            type = reader.GetString(0),
                            amount = reader.GetDecimal(1)
                        });
                    }

                    // Check and add type2
                    if (!reader.IsDBNull(2) && !reader.IsDBNull(3))
                    {
                        types.Add(new
                        {
                            type = reader.GetString(2),
                            amount = reader.GetDecimal(3)
                        });
                    }

                    // Check and add type3
                    if (!reader.IsDBNull(4) && !reader.IsDBNull(5))
                    {
                        types.Add(new
                        {
                            type = reader.GetString(4),
                            amount = reader.GetDecimal(5)
                        });
                    }

                    // Check and add type4
                    if (!reader.IsDBNull(6) && !reader.IsDBNull(7))
                    {
                        types.Add(new
                        {
                            type = reader.GetString(6),
                            amount = reader.GetDecimal(7)
                        });
                    }

                    return Ok(new { 
                        types,
                        advance_payment_enabled = reader.IsDBNull(8) ? (bool?)null : (reader.GetInt32(8) == 1),
                        default_advance_percentage = reader.IsDBNull(9) ? (decimal?)null : reader.GetDecimal(9),
                        hall_name = reader.GetString(10)
                    });
                }

                return NotFound(new { message = "Hall not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // [HttpGet("all-halls")]
        // public async Task<IActionResult> GetAllHalls()
        // {
        //     try
        //     {
        //         using var connection = _dbConnection.GetConnection();
        //         await connection.OpenAsync();

        //         string query = @"
        //             SELECT id, admin_id, admin_name, hall_name, 
        //                    type1, type1_amount, type2, type2_amount,
        //                    type3, type3_amount, type4, type4_amount,
        //                    created_at, updated_at
        //             FROM settings";

        //         using var cmd = new NpgsqlCommand(query, connection);
        //         using var reader = await cmd.ExecuteReaderAsync();

        //         var halls = new List<object>();

        //         while (await reader.ReadAsync())
        //         {
        //             var types = new List<object>();

        //             // Check and add type1
        //             if (!reader.IsDBNull(4) && !reader.IsDBNull(5))
        //             {
        //                 types.Add(new
        //                 {
        //                     type = reader.GetString(4),
        //                     amount = reader.GetDecimal(5)
        //                 });
        //             }

        //             // Check and add type2
        //             if (!reader.IsDBNull(6) && !reader.IsDBNull(7))
        //             {
        //                 types.Add(new
        //                 {
        //                     type = reader.GetString(6),
        //                     amount = reader.GetDecimal(7)
        //                 });
        //             }

        //             // Check and add type3
        //             if (!reader.IsDBNull(8) && !reader.IsDBNull(9))
        //             {
        //                 types.Add(new
        //                 {
        //                     type = reader.GetString(8),
        //                     amount = reader.GetDecimal(9)
        //                 });
        //             }

        //             // Check and add type4
        //             if (!reader.IsDBNull(10) && !reader.IsDBNull(11))
        //             {
        //                 types.Add(new
        //                 {
        //                     type = reader.GetString(10),
        //                     amount = reader.GetDecimal(11)
        //                 });
        //             }

        //             halls.Add(new
        //             {
        //                 id = reader.GetInt32(0),
        //                 admin_id = reader.GetString(1),
        //                 admin_name = reader.GetString(2),
        //                 hall_name = reader.GetString(3),
        //                 types = types,
        //                 created_at = reader.GetDateTime(12),
        //                 updated_at = reader.GetDateTime(13)
        //             });
        //         }

        //         return Ok(new { halls });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        //     }
        // }
    }
}
