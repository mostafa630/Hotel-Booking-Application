using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Hotel_Booking_Application.Connection
{
    public class SqlConnectionFactory(IConfiguration configuration)
    {
        private readonly string _connectionString =
            configuration["ConnectionString"] ?? throw new InvalidOperationException("ConnectionString not found");
        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
