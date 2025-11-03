using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json.Serialization;

namespace RAILWAY_BACKEND.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BookingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region Create Bookings (Single Online Booking)

        [HttpPost("online-book")]
        public IActionResult CreateOnlineBooking([FromBody] NewBookingRequest booking)
        {
            if (booking == null)
                return BadRequest(new { message = "No booking provided" });

            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            // Step 1: Get admin_id for the worker
            string adminId;
            using (var getAdminCmd = new NpgsqlCommand(
                "SELECT admin_id FROM worker_accounts WHERE worker_id = @workerId", connection))
            {
                getAdminCmd.Parameters.AddWithValue("@workerId", booking.WorkerId);
                var result = getAdminCmd.ExecuteScalar();

                if (result == null)
                    return BadRequest(new { message = $"Invalid worker_id {booking.WorkerId}. No admin found." });

                adminId = result.ToString()!;
            }

            // Step 2: Prepare insert query (includes booking_id)
            string insertQuery = @"
                INSERT INTO bookings 
                (booking_id, admin_id, worker_id, guest_name, phone_number, number_of_persons, 
                 booking_type, total_hours, booking_date, in_time, out_time, 
                 proof_type, proof_id, price_per_person, total_amount, paid_amount, 
                 balance_amount, payment_method, status)
                VALUES 
                (@booking_id, @admin_id, @worker_id, @guest_name, @phone_number, @number_of_persons, 
                 @booking_type, @total_hours, @booking_date, @in_time, @out_time, 
                 @proof_type, @proof_id, @price_per_person, @total_amount, @paid_amount, 
                 @balance_amount, @payment_method, @status);";

            string status = string.IsNullOrEmpty(booking.Status) ? "Active" : booking.Status;

            using var cmd = new NpgsqlCommand(insertQuery, connection);
            cmd.Parameters.AddWithValue("@booking_id", booking.BookingId);
            cmd.Parameters.AddWithValue("@admin_id", adminId);
            cmd.Parameters.AddWithValue("@worker_id", booking.WorkerId);
            cmd.Parameters.AddWithValue("@guest_name", booking.GuestName);
            cmd.Parameters.AddWithValue("@phone_number", booking.PhoneNumber);
            cmd.Parameters.AddWithValue("@number_of_persons", booking.NumberOfPersons);
            cmd.Parameters.AddWithValue("@booking_type", booking.BookingType);
            cmd.Parameters.AddWithValue("@total_hours", booking.TotalHours);
            cmd.Parameters.AddWithValue("@booking_date", booking.BookingDate);
            cmd.Parameters.AddWithValue("@in_time", booking.InTime);
            cmd.Parameters.AddWithValue("@out_time", booking.OutTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@proof_type", booking.ProofType);
            cmd.Parameters.AddWithValue("@proof_id", booking.ProofId);
            cmd.Parameters.AddWithValue("@price_per_person", booking.PricePerPerson);
            cmd.Parameters.AddWithValue("@total_amount", booking.TotalAmount);
            cmd.Parameters.AddWithValue("@paid_amount", booking.PaidAmount);
            cmd.Parameters.AddWithValue("@balance_amount", booking.BalanceAmount);
            cmd.Parameters.AddWithValue("@payment_method", booking.PaymentMethod);
            cmd.Parameters.AddWithValue("@status", status);

            cmd.ExecuteNonQuery();

            return Ok(new
            {
                message = "Booking created successfully",
                booking_id = booking.BookingId
            });
        }

        [HttpPost("create")]
        public IActionResult CreateBookings([FromBody] List<NewBookingRequest> bookings)
        {
            if (bookings == null || bookings.Count == 0)
                return BadRequest(new { message = "No bookings provided" });

            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            var createdBookings = new List<object>();

            foreach (var request in bookings)
            {
            // Step 1: Find admin_id for the given worker
                int adminId;
                using (var getAdminCmd = new NpgsqlCommand(
                    "SELECT admin_id FROM worker_accounts WHERE worker_id = @workerId", connection))
                {
                    getAdminCmd.Parameters.AddWithValue("@workerId", request.WorkerId);
                    var result = getAdminCmd.ExecuteScalar();
                    if (result == null)
                        return BadRequest(new { message = $"Invalid worker_id {request.WorkerId}. No admin found." });

                    adminId = Convert.ToInt32(result);
                }

                // Step 2: Insert with booking_id (manually)
                string insertQuery = @"
                    INSERT INTO bookings 
                    (booking_id, admin_id, worker_id, guest_name, phone_number, number_of_persons, 
                    booking_type, total_hours, booking_date, in_time, out_time,
                    proof_type, proof_id, price_per_person, total_amount, paid_amount, 
                    balance_amount, payment_method, status)
                    VALUES 
                    (@booking_id, @admin_id, @worker_id, @guest_name, @phone_number, @number_of_persons, 
                    @booking_type, @total_hours, @booking_date, @in_time, @out_time,
                    @proof_type, @proof_id, @price_per_person, @total_amount, @paid_amount, 
                    @balance_amount, @payment_method, @status);";

                string status = string.IsNullOrEmpty(request.Status) ? "Active" : request.Status;

                using var cmd = new NpgsqlCommand(insertQuery, connection);
                cmd.Parameters.AddWithValue("@booking_id", request.BookingId);
                cmd.Parameters.AddWithValue("@admin_id", adminId);
                cmd.Parameters.AddWithValue("@worker_id", request.WorkerId);
                cmd.Parameters.AddWithValue("@guest_name", request.GuestName);
                cmd.Parameters.AddWithValue("@phone_number", request.PhoneNumber);
                cmd.Parameters.AddWithValue("@number_of_persons", request.NumberOfPersons);
                cmd.Parameters.AddWithValue("@booking_type", request.BookingType);
                cmd.Parameters.AddWithValue("@total_hours", request.TotalHours);
                cmd.Parameters.AddWithValue("@booking_date", request.BookingDate);
                cmd.Parameters.AddWithValue("@in_time", request.InTime);
                cmd.Parameters.AddWithValue("@out_time", request.OutTime ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@proof_type", request.ProofType);
                cmd.Parameters.AddWithValue("@proof_id", request.ProofId);
                cmd.Parameters.AddWithValue("@price_per_person", request.PricePerPerson);
                cmd.Parameters.AddWithValue("@total_amount", request.TotalAmount);
                cmd.Parameters.AddWithValue("@paid_amount", request.PaidAmount);
                cmd.Parameters.AddWithValue("@balance_amount", request.BalanceAmount);
                cmd.Parameters.AddWithValue("@payment_method", request.PaymentMethod);
                cmd.Parameters.AddWithValue("@status", status);

                cmd.ExecuteNonQuery();

                createdBookings.Add(new
            {
            booking_id = request.BookingId,
            admin_id = adminId,
            worker_id = request.WorkerId,
            status = status
        });
    }

    return Ok(new
    {
        message = "Bookings created successfully",
        bookings = createdBookings
    });
}


        #endregion

        #region Checkout / Complete Booking

        [HttpPut("checkout")]
        public IActionResult Checkout([FromBody] CheckoutRequest request)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            string updateQuery = @"
                UPDATE bookings 
                SET out_time = @out_time,
                    status = @status,
                    updated_at = CURRENT_TIMESTAMP
                WHERE booking_id = @booking_id;";

            using var cmd = new NpgsqlCommand(updateQuery, connection);
            TimeSpan outTime = request.OutTime ?? DateTime.Now.TimeOfDay;
            string status = string.IsNullOrEmpty(request.Status) ? "Completed" : request.Status;

            cmd.Parameters.AddWithValue("@out_time", outTime);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@booking_id", request.BookingId);

            int rows = cmd.ExecuteNonQuery();
            if (rows == 0)
                return NotFound(new { message = "Booking not found" });

            return Ok(new
            {
                message = "Checkout completed",
                booking_id = request.BookingId,
                out_time = outTime.ToString(),
                status = status
            });
        }

        #endregion

        #region Get Bookings

        [HttpGet("{id}")]
        public IActionResult GetBooking(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            string query = "SELECT * FROM bookings WHERE booking_id = @id";
            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return NotFound(new { message = "Booking not found" });

            var booking = new
            {
                booking_id = reader["booking_id"],
                guest_name = reader["guest_name"],
                phone_number = reader["phone_number"],
                booking_date = reader["booking_date"],
                in_time = reader["in_time"],
                out_time = reader["out_time"],
                status = reader["status"]
            };

            return Ok(booking);
        }

        [HttpGet("active/{id}")]
        public IActionResult GetActiveBookings(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            string query = @"
                SELECT booking_id, guest_name, phone_number, booking_date, in_time, status 
                FROM bookings 
                WHERE status = 'Active' AND worker_id = @worker_id
                ORDER BY booking_date DESC";

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@worker_id", id);

            using var reader = cmd.ExecuteReader();

            var activeBookings = new List<object>();
            while (reader.Read())
            {
                activeBookings.Add(new
                {
                    booking_id = reader["booking_id"],
                    guest_name = reader["guest_name"],
                    phone_number = reader["phone_number"],
                    booking_date = reader["booking_date"],
                    in_time = reader["in_time"],
                    status = reader["status"]
                });
            }

            return Ok(activeBookings);
        }

        #endregion

        #region Request Models

        public class CheckoutRequest
        {
            public int BookingId { get; set; }
            public TimeSpan? OutTime { get; set; }
            public string? Status { get; set; }
        }

        public class NewBookingRequest
        {
            [JsonPropertyName("booking_id")]
            public string BookingId { get; set; } = "";

            [JsonPropertyName("worker_id")]
            public string WorkerId { get; set; } = "";

            [JsonPropertyName("guest_name")]
            public string GuestName { get; set; } = "";

            [JsonPropertyName("phone_number")]
            public string PhoneNumber { get; set; } = "";

            [JsonPropertyName("number_of_persons")]
            public int NumberOfPersons { get; set; }

            [JsonPropertyName("booking_type")]
            public string BookingType { get; set; } = "";

            [JsonPropertyName("total_hours")]
            public int TotalHours { get; set; }

            [JsonPropertyName("booking_date")]
            public DateTime BookingDate { get; set; }

            [JsonPropertyName("in_time")]
            public TimeSpan InTime { get; set; }

            [JsonPropertyName("out_time")]
            public TimeSpan? OutTime { get; set; }

            [JsonPropertyName("proof_type")]
            public string ProofType { get; set; } = "";

            [JsonPropertyName("proof_id")]
            public string ProofId { get; set; } = "";

            [JsonPropertyName("price_per_person")]
            public decimal PricePerPerson { get; set; }

            [JsonPropertyName("total_amount")]
            public decimal TotalAmount { get; set; }

            [JsonPropertyName("paid_amount")]
            public decimal PaidAmount { get; set; }

            [JsonPropertyName("balance_amount")]
            public decimal BalanceAmount { get; set; }

            [JsonPropertyName("payment_method")]
            public string PaymentMethod { get; set; } = "Cash";

            [JsonPropertyName("status")]
            public string? Status { get; set; }
        }

        #endregion
    }
}
