using Azure.Core;
using Hotel_Booking_Application.Connection;
using Hotel_Booking_Application.DTOs.RoomDTOs;
using Hotel_Booking_Application.DTOs.RoomDTOs.RoomTypeDTOs;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Hotel_Booking_Application.Repositories
{
    public class RoomRepository(SqlConnectionFactory _connectionFactory)
    {
        public async Task<CreateRoomResponseDTO> CreateRoomAsync(CreateRoomDTO createRoomDTO)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("CreateRoom", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // set in the in parameters of the procedure
            command.Parameters.AddWithValue("@RoomNumber", createRoomDTO.RoomNumber);
            command.Parameters.AddWithValue("@RoomTypeID", createRoomDTO.RoomTypeID);
            command.Parameters.AddWithValue("@Price", createRoomDTO.Price);
            command.Parameters.AddWithValue("@BedType", createRoomDTO.BedType);
            command.Parameters.AddWithValue("@ViewType", createRoomDTO.ViewType);
            command.Parameters.AddWithValue("@Status", createRoomDTO.Status);
            command.Parameters.AddWithValue("@IsActive", createRoomDTO.IsActive);
            command.Parameters.AddWithValue("@CreatedBy", "System");

            // set the out paramteres from the procedure
            var roomIdParameter = new SqlParameter
            {
                ParameterName = "@NewRoomID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();

            command.Parameters.Add(roomIdParameter);
            command.Parameters.Add(statusCodeParameter);
            command.Parameters.Add(messageParameter);

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var roomId = (int)roomIdParameter.Value;
                var isCreated = (int)statusCodeParameter.Value == 0;
                var message = messageParameter.Value.ToString();

                return new CreateRoomResponseDTO
                {
                    RoomID = roomId,
                    Message = message,
                    IsCreated = isCreated
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating room: {ex.Message}", ex);
            }
        }

        public async Task<UpdateRoomResponseDTO> UpdateRoomAsync(UpdateRoomDTO updateRoomDTO)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("UpdateRoom", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // set in parameters of the procedure
            command.Parameters.AddWithValue("@RoomID", updateRoomDTO.RoomID);
            command.Parameters.AddWithValue("@RoomNumber", updateRoomDTO.RoomNumber);
            command.Parameters.AddWithValue("@RoomTypeID", updateRoomDTO.RoomTypeID);
            command.Parameters.AddWithValue("@Price", updateRoomDTO.Price);
            command.Parameters.AddWithValue("@BedType", updateRoomDTO.BedType);
            command.Parameters.AddWithValue("@ViewType", updateRoomDTO.ViewType);
            command.Parameters.AddWithValue("@Status", updateRoomDTO.Status);
            command.Parameters.AddWithValue("@IsActive", updateRoomDTO.IsActive);
            command.Parameters.AddWithValue("@ModifiedBy", "System");

            // set the out parameters from the procedure
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();

            command.Parameters.Add(statusCodeParameter);
            command.Parameters.Add(messageParameter);

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var isUpdated = (int)statusCodeParameter.Value == 0;
                var message = messageParameter.Value.ToString();
                return new UpdateRoomResponseDTO
                {
                    RoomId = updateRoomDTO.RoomID,
                    IsUpdated = isUpdated,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating room: {ex.Message}", ex);
            }

        }

        public async Task<DeleteRoomResponseDTO> DeleteRoomAsync(int roomId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("DeleteRoom", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            // set in parameters of the procedure
            command.Parameters.AddWithValue("@RoomID", roomId);

            // set the out parameters from the procedure
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();

            // add the out parameters to the command
            command.Parameters.Add(statusCodeParameter);
            command.Parameters.Add(messageParameter);

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var isDeleted = (int)statusCodeParameter.Value == 0;
                var message = messageParameter.Value.ToString();
                return new DeleteRoomResponseDTO
                {
                    IsDeleted = isDeleted,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting room: {ex.Message}", ex);
            }
        }

        public async Task<RoomDetailsResponseDTO> GetRoomByIdAsync(int roomId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("GetRoomById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@RoomID", roomId);
            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new RoomDetailsResponseDTO
                    {
                        RoomID = reader.GetInt32("RoomID"),
                        RoomNumber = reader.GetString("RoomNumber"),
                        RoomTypeID = reader.GetInt32("RoomTypeID"),
                        Price = reader.GetDecimal("Price"),
                        BedType = reader.GetString("BedType"),
                        ViewType = reader.GetString("ViewType"),
                        Status = reader.GetString("Status"),
                        IsActive = reader.GetBoolean("IsActive")
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                // we can modify it with a business need
                throw new Exception($"Error retrieving room by ID: {ex.Message}", ex);
            }

        }

        public async Task<List<RoomDetailsResponseDTO>> GetAllRoomsAsync(GetAllRoomsDTO getAllRoomsDTO)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("GetAllRooms", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            // set in parameters of the procedure
            command.Parameters.Add(new SqlParameter("@RoomTypeID", SqlDbType.Int)
            {
                Value = getAllRoomsDTO.RoomTypeID.HasValue ? (object)getAllRoomsDTO.RoomTypeID.Value : DBNull.Value
            });
            command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50)
            {
                Value = string.IsNullOrEmpty(getAllRoomsDTO.Status) ? DBNull.Value : (object)getAllRoomsDTO.Status
            });
            try
            {
                await connection.OpenAsync();
                var rooms = new List<RoomDetailsResponseDTO>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rooms.Add(new RoomDetailsResponseDTO
                    {
                        RoomID = reader.GetInt32("RoomID"),
                        RoomNumber = reader.GetString("RoomNumber"),
                        RoomTypeID = reader.GetInt32("RoomTypeID"),
                        Price = reader.GetDecimal("Price"),
                        BedType = reader.GetString("BedType"),
                        ViewType = reader.GetString("ViewType"),
                        Status = reader.GetString("Status"),
                        IsActive = reader.GetBoolean("IsActive")
                    });
                }
                return rooms;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all rooms: {ex.Message}", ex);
            }


        }
        private SqlParameter getStatusCodeParameter()
        {
            return new SqlParameter
            {
                ParameterName = "@StatusCode",
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
