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

        [HttpPost("add-hall")]
        public async Task<IActionResult> AddHall([FromBody] HallDetailsDto hallDetails)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(hallDetails.AdminId))
                {
                    return BadRequest(new { message = "admin_id is required" });
                }
                if (string.IsNullOrWhiteSpace(hallDetails.AdminName))
                {
                    return BadRequest(new { message = "admin_name is required" });
                }
                if (string.IsNullOrWhiteSpace(hallDetails.HallName))
                {
                    return BadRequest(new { message = "hall_name is required" });
                }

                using var connection = _dbConnection.GetConnection();
                await connection.OpenAsync();

                string query = @"
                    INSERT INTO settings 
                    (admin_id, admin_name, hall_name, type1, type1_amount, 
                     type2, type2_amount, type3, type3_amount, type4, type4_amount)
                    VALUES 
                    (@admin_id, @admin_name, @hall_name, @type1, @type1_amount, 
                     @type2, @type2_amount, @type3, @type3_amount, @type4, @type4_amount)
                    RETURNING id";

                using var cmd = new NpgsqlCommand(query, connection);
                
                cmd.Parameters.AddWithValue("@admin_id", hallDetails.AdminId);
                cmd.Parameters.AddWithValue("@admin_name", hallDetails.AdminName);
                cmd.Parameters.AddWithValue("@hall_name", hallDetails.HallName);
                
                // Handle nullable fields
                cmd.Parameters.AddWithValue("@type1", (object?)hallDetails.Type1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type1_amount", (object?)hallDetails.Type1Amount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type2", (object?)hallDetails.Type2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type2_amount", (object?)hallDetails.Type2Amount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type3", (object?)hallDetails.Type3 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type3_amount", (object?)hallDetails.Type3Amount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type4", (object?)hallDetails.Type4 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type4_amount", (object?)hallDetails.Type4Amount ?? DBNull.Value);

                var newId = await cmd.ExecuteScalarAsync();

                return Ok(new 
                { 
                    message = "Hall details added successfully", 
                    id = newId 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPut("update-hall/{adminId}")]
        public async Task<IActionResult> UpdateHall(string adminId, [FromBody] HallDetailsDto hallDetails)
        {
            try
            {
                using var connection = _dbConnection.GetConnection();
                await connection.OpenAsync();

                // Check if hall exists for this admin
                string checkQuery = "SELECT COUNT(*) FROM settings WHERE admin_id = @adminId";
                using var checkCmd = new NpgsqlCommand(checkQuery, connection);
                checkCmd.Parameters.AddWithValue("@adminId", adminId);
                var exists = (long)(await checkCmd.ExecuteScalarAsync() ?? 0) > 0;

                if (!exists)
                {
                    return NotFound(new { message = "Hall not found for this admin" });
                }

                string query = @"
                    UPDATE settings 
                    SET admin_name = COALESCE(@admin_name, admin_name),
                        hall_name = COALESCE(@hall_name, hall_name),
                        type1 = @type1,
                        type1_amount = @type1_amount,
                        type2 = @type2,
                        type2_amount = @type2_amount,
                        type3 = @type3,
                        type3_amount = @type3_amount,
                        type4 = @type4,
                        type4_amount = @type4_amount,
                        updated_at = CURRENT_TIMESTAMP
                    WHERE admin_id = @adminId";

                using var cmd = new NpgsqlCommand(query, connection);
                
                cmd.Parameters.AddWithValue("@adminId", adminId);
                cmd.Parameters.AddWithValue("@admin_name", (object?)hallDetails.AdminName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@hall_name", (object?)hallDetails.HallName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type1", (object?)hallDetails.Type1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type1_amount", (object?)hallDetails.Type1Amount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type2", (object?)hallDetails.Type2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type2_amount", (object?)hallDetails.Type2Amount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type3", (object?)hallDetails.Type3 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type3_amount", (object?)hallDetails.Type3Amount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type4", (object?)hallDetails.Type4 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@type4_amount", (object?)hallDetails.Type4Amount ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { message = "Hall details updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpDelete("delete-hall/{adminId}")]
        public async Task<IActionResult> DeleteHall(string adminId)
        {
            try
            {
                using var connection = _dbConnection.GetConnection();
                await connection.OpenAsync();

                string query = "DELETE FROM settings WHERE admin_id = @adminId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@adminId", adminId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Hall not found for this admin" });
                }

                return Ok(new { message = "Hall deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
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
                           type3, type3_amount, type4, type4_amount
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

                    return Ok(new { types });
                }

                return NotFound(new { message = "Hall not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("all-halls")]
        public async Task<IActionResult> GetAllHalls()
        {
            try
            {
                using var connection = _dbConnection.GetConnection();
                await connection.OpenAsync();

                string query = @"
                    SELECT id, admin_id, admin_name, hall_name, 
                           type1, type1_amount, type2, type2_amount,
                           type3, type3_amount, type4, type4_amount,
                           created_at, updated_at
                    FROM settings";

                using var cmd = new NpgsqlCommand(query, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                var halls = new List<object>();

                while (await reader.ReadAsync())
                {
                    var types = new List<object>();

                    // Check and add type1
                    if (!reader.IsDBNull(4) && !reader.IsDBNull(5))
                    {
                        types.Add(new
                        {
                            type = reader.GetString(4),
                            amount = reader.GetDecimal(5)
                        });
                    }

                    // Check and add type2
                    if (!reader.IsDBNull(6) && !reader.IsDBNull(7))
                    {
                        types.Add(new
                        {
                            type = reader.GetString(6),
                            amount = reader.GetDecimal(7)
                        });
                    }

                    // Check and add type3
                    if (!reader.IsDBNull(8) && !reader.IsDBNull(9))
                    {
                        types.Add(new
                        {
                            type = reader.GetString(8),
                            amount = reader.GetDecimal(9)
                        });
                    }

                    // Check and add type4
                    if (!reader.IsDBNull(10) && !reader.IsDBNull(11))
                    {
                        types.Add(new
                        {
                            type = reader.GetString(10),
                            amount = reader.GetDecimal(11)
                        });
                    }

                    halls.Add(new
                    {
                        id = reader.GetInt32(0),
                        admin_id = reader.GetString(1),
                        admin_name = reader.GetString(2),
                        hall_name = reader.GetString(3),
                        types = types,
                        created_at = reader.GetDateTime(12),
                        updated_at = reader.GetDateTime(13)
                    });
                }

                return Ok(new { halls });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}
