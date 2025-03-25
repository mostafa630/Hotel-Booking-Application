using Hotel_Booking_Application.Connection;
using Hotel_Booking_Application.DTOs.AmenityDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.WebSockets;
using System.Security.Cryptography.Xml;

namespace Hotel_Booking_Application.Repositories
{
    public class AmenityRepository(SqlConnectionFactory _connectionFactory)
    {
        public async Task<AmenityFetchResultDTO> FetchAmenitiesAsync(bool? isActive)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("FetchAmenities", conn);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IsActive", (object)isActive ?? DBNull.Value);

            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();
            command.Parameters.Add(statusCodeParameter);
            command.Parameters.Add(messageParameter);

            await conn.OpenAsync();

            var amenities = new List<AmenityDetailsDTO>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    amenities.Add(new AmenityDetailsDTO
                    {
                        AmenityID = reader.GetInt32(reader.GetOrdinal("AmenityID")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }
            }

            return new AmenityFetchResultDTO
            {
                Amenities = amenities,
                IsSuccess = Convert.ToBoolean(statusCodeParameter.Value),
                Message = messageParameter.Value.ToString()
            };
        }

        public async Task<AmenityDetailsDTO> FetchAmenityByIdAsync(int amenityId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("FetchAmenityByID", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@AmenityID", amenityId);

            await connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AmenityDetailsDTO
                {
                    AmenityID = reader.GetInt32(reader.GetOrdinal("AmenityID")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                };
            }
            else
            {
                return null;
            }
        }

        public async Task<AmenityInsertResponseDTO> AddAmenityAsync(AmenityInsertDTO amenity)
        {
            AmenityInsertResponseDTO amenityInsertResponseDTO = new AmenityInsertResponseDTO();
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("AddAmenity", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Name", amenity.Name);
            command.Parameters.AddWithValue("@Description", amenity.Description);
            command.Parameters.AddWithValue("@CreatedBy", "System");

            var amenityIdParameter = new SqlParameter
            {
                ParameterName = "@AmenityID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();

            command.Parameters.AddRange(new SqlParameter[] { amenityIdParameter, statusCodeParameter, messageParameter });

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var res = Convert.ToBoolean(statusCodeParameter.Value);

                if (res)
                {
                    amenityInsertResponseDTO.AmenityID = Convert.ToInt32(amenityIdParameter.Value);
                    amenityInsertResponseDTO.Message = Convert.ToString(messageParameter.Value);
                    amenityInsertResponseDTO.IsCreated = true;
                    return amenityInsertResponseDTO;
                }
                else
                {
                    amenityInsertResponseDTO.Message = Convert.ToString(messageParameter.Value);
                    amenityInsertResponseDTO.IsCreated = false;
                    return amenityInsertResponseDTO;
                }
            }

            catch (SqlException ex)
            {
                amenityInsertResponseDTO.Message = ex.Message;
                amenityInsertResponseDTO.IsCreated = false;
                return amenityInsertResponseDTO;
            }

        }

        public async Task<AmenityUpdateResponseDTO> UpdateAmenityAsync(AmenityUpdateDTO amenity)
        {
            AmenityUpdateResponseDTO amenityUpdateResponseDTO = new AmenityUpdateResponseDTO()
            {
                AmenityID = amenity.AmenityID
            };
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("UpdateAmenity", connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@AmenityID", amenity.AmenityID);
            command.Parameters.AddWithValue("@Name", amenity.Name);
            command.Parameters.AddWithValue("@Description", amenity.Description);
            command.Parameters.AddWithValue("@IsActive", amenity.IsActive);
            command.Parameters.AddWithValue("@ModifiedBy", "System");

            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();
            command.Parameters.AddRange(new SqlParameter[] { statusCodeParameter, messageParameter });

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                amenityUpdateResponseDTO.Message = messageParameter.Value.ToString();
                amenityUpdateResponseDTO.IsUpdated = Convert.ToBoolean(statusCodeParameter.Value);
                return amenityUpdateResponseDTO;
            }
            catch (SqlException ex)
            {
                amenityUpdateResponseDTO.Message = ex.Message;
                amenityUpdateResponseDTO.IsUpdated = false;
                return amenityUpdateResponseDTO;
            }
        }

        public async Task<AmenityDeleteResponseDTO> DeleteAmenityAsync(int amenityId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("DeleteAmenity", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@AmenityID", amenityId);

            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();
            command.Parameters.AddRange(new SqlParameter[] { statusCodeParameter, messageParameter });

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return new AmenityDeleteResponseDTO
            {
                IsDeleted = Convert.ToBoolean(statusCodeParameter.Value),
                Message = messageParameter.Value.ToString()
            };
        }

        public async Task<AmenityBulkOperationResultDTO> BulkInsertAmenitiesAsync(List<AmenityInsertDTO> amenities)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("BulkInsertAmenities", connection);
            command.CommandType = CommandType.StoredProcedure;

            var amenitiesTable = new DataTable();
            amenitiesTable.Columns.Add("Name", typeof(string));
            amenitiesTable.Columns.Add("Description", typeof(string));
            amenitiesTable.Columns.Add("CreatedBy", typeof(string));
            foreach (var amenity in amenities)
            {
                amenitiesTable.Rows.Add(amenity.Name, amenity.Description, "System");
            }
            var param = command.Parameters.AddWithValue("@Amenities", amenitiesTable);
            param.SqlDbType = SqlDbType.Structured;

            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();
            command.Parameters.AddRange(new SqlParameter[] { statusCodeParameter, messageParameter });

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            return new AmenityBulkOperationResultDTO
            {
                IsSuccess = Convert.ToBoolean(statusCodeParameter.Value),
                Message = messageParameter.Value.ToString()
            };
        }

        public async Task<AmenityBulkOperationResultDTO> BulkUpdateAmenitiesAsync(List<AmenityUpdateDTO> amenities)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("BulkUpdateAmenities", connection);
            command.CommandType = CommandType.StoredProcedure;
            var amenitiesTable = new DataTable();
            amenitiesTable.Columns.Add("AmenityID", typeof(int));
            amenitiesTable.Columns.Add("Name", typeof(string));
            amenitiesTable.Columns.Add("Description", typeof(string));
            amenitiesTable.Columns.Add("IsActive", typeof(bool));
            foreach (var amenity in amenities)
            {
                amenitiesTable.Rows.Add(amenity.AmenityID, amenity.Name, amenity.Description, amenity.IsActive);
            }
            var param = command.Parameters.AddWithValue("@AmenityUpdates", amenitiesTable);
            param.SqlDbType = SqlDbType.Structured;
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();
            command.Parameters.AddRange(new SqlParameter[] { statusCodeParameter, messageParameter });

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return new AmenityBulkOperationResultDTO
            {
                IsSuccess = Convert.ToBoolean(statusCodeParameter.Value),
                Message = messageParameter.Value.ToString()
            };
        }
        public async Task<AmenityBulkOperationResultDTO> BulkUpdateAmenityStatusAsync(List<AmenityStatusDTO> amenityStatuses)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("BulkUpdateAmenityStatus", connection);
            command.CommandType = CommandType.StoredProcedure;
            var amenityStatusTable = new DataTable();
            amenityStatusTable.Columns.Add("AmenityID", typeof(int));
            amenityStatusTable.Columns.Add("IsActive", typeof(bool));
            foreach (var status in amenityStatuses)
            {
                amenityStatusTable.Rows.Add(status.AmenityID, status.IsActive);
            }
            var param = command.Parameters.AddWithValue("@AmenityStatuses", amenityStatusTable);
            param.SqlDbType = SqlDbType.Structured;
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();
            command.Parameters.AddRange(new SqlParameter[] { statusCodeParameter, messageParameter });

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return new AmenityBulkOperationResultDTO
            {
                IsSuccess = Convert.ToBoolean(statusCodeParameter.Value),
                Message = messageParameter.Value.ToString()
            };
        }


        private SqlParameter getStatusCodeParameter()
        {
            return new SqlParameter
            {
                ParameterName = "@Status",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
        }
        private SqlParameter getMessageParameter()
        {
            return new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType = SqlDbType.NVarChar,
                Size = 255,
                Direction = ParameterDirection.Output
            };
        }
    }
}
