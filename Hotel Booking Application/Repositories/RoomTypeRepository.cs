using Hotel_Booking_Application.Connection;
using Hotel_Booking_Application.DTOs.RoomDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.WebSockets;

namespace Hotel_Booking_Application.Repositories
{
    public class RoomTypeRepository(SqlConnectionFactory _connectionFactory)
    {
        public async Task<List<RoomTypeResponseDTO>> RetrieveAllRoomTypesAsync(bool? isActice)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("GetAllRoomTypes", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure.
            command.Parameters.AddWithValue("@IsActive", isActice ?? (object)DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var roomTypes = new List<RoomTypeResponseDTO>();

            while (reader.Read())
            {
                var roomType = new RoomTypeResponseDTO
                {
                    RoomTypeID = reader.GetInt32("RoomTypeID"),
                    TypeName = reader.GetString("TypeName"),
                    AccessibilityFeatures = reader.GetString("AccessibilityFeatures"),
                    Description = reader.GetString("Description"),
                    IsActive = reader.GetBoolean("IsActive")
                };
                roomTypes.Add(roomType);
            }
            return roomTypes;
        }

        public async Task<RoomTypeResponseDTO> RetrieveRoomTypeByIdAsync(int roomTypeId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("GetRoomTypeById", connection);
            command.CommandType = CommandType.StoredProcedure;
            //set the in parameters of the procedure.
            command.Parameters.AddWithValue("@RoomTypeID", roomTypeId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (!reader.Read())
            {
                return null;
            }

            var roomType = new RoomTypeResponseDTO
            {
                RoomTypeID = reader.GetInt32("RoomTypeID"),
                TypeName = reader.GetString("TypeName"),
                AccessibilityFeatures = reader.GetString("AccessibilityFeatures"),
                Description = reader.GetString("Description"),
                IsActive = reader.GetBoolean("IsActive")
            };

            return roomType;
        }

        public async Task<CreateRoomTypeResponseDTO> CreateRoomType(CreateRoomTypeDTO createRoomTypeDTO)
        {
            var CreateRoomTypeResponseDTO = new CreateRoomTypeResponseDTO();

            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("CreateRoomType", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set input parameters of the procedure.  
            command.Parameters.AddWithValue("@TypeName", createRoomTypeDTO.TypeName);
            command.Parameters.AddWithValue("@AccessibilityFeatures", createRoomTypeDTO.AccessibilityFeatures);
            command.Parameters.AddWithValue("@Description", createRoomTypeDTO.Description);
            command.Parameters.AddWithValue("@createdBy", "System");

            // set output parameters of the procedure.
            var RoomTypeIdParameter = new SqlParameter
            {
                ParameterName = "@NewRoomTypeID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();

            command.Parameters.Add(RoomTypeIdParameter);
            command.Parameters.Add(statusCodeParameter);
            command.Parameters.Add(messageParameter);

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                var statusCode = (int)statusCodeParameter.Value;
                if (statusCode == 0)
                {
                    CreateRoomTypeResponseDTO.RoomTypeId = (int)RoomTypeIdParameter.Value;
                    CreateRoomTypeResponseDTO.Message = messageParameter.Value.ToString();
                    CreateRoomTypeResponseDTO.IsCreated = true;

                    return CreateRoomTypeResponseDTO;
                }

                CreateRoomTypeResponseDTO.Message = messageParameter.Value.ToString();
                CreateRoomTypeResponseDTO.IsCreated = false;

                return CreateRoomTypeResponseDTO;
            }
            catch (Exception ex)
            {
                CreateRoomTypeResponseDTO.Message = ex.Message;
                CreateRoomTypeResponseDTO.IsCreated = false;

                return CreateRoomTypeResponseDTO;
            }
        }

        public async Task<UpdateRoomTypeResponseDTO> UpdateRoomType(UpdateRoomTypeDTO updateRoomTypeDTO)
        {
            var updateRoomTypeResponseDTO = new UpdateRoomTypeResponseDTO
            {
                RoomTypeId = updateRoomTypeDTO.RoomTypeID
            };

            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("UpdateRoomType", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set input parameters of the procedure.
            command.Parameters.AddWithValue("@RoomTypeID", updateRoomTypeDTO.RoomTypeID);
            command.Parameters.AddWithValue("@TypeName", updateRoomTypeDTO.TypeName);
            command.Parameters.AddWithValue("@AccessibilityFeatures", updateRoomTypeDTO.AccessibilityFeatures);
            command.Parameters.AddWithValue("@Description", updateRoomTypeDTO.Description);
            command.Parameters.AddWithValue("@ModifiedBy", "System");
            // set out parameter of the Procedure
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();

            command.Parameters.Add(statusCodeParameter);
            command.Parameters.Add(messageParameter);

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                updateRoomTypeResponseDTO.Message = messageParameter.Value.ToString();
                updateRoomTypeResponseDTO.IsUpdated = (int)statusCodeParameter.Value == 0;

                return updateRoomTypeResponseDTO;
            }
            catch (Exception ex)
            {
                updateRoomTypeResponseDTO.Message = ex.Message;
                updateRoomTypeResponseDTO.IsUpdated = false;
                return updateRoomTypeResponseDTO;
            }
        }

        public async Task<DeleteRoomTypeResponseDTO> DeleteRoomType(int roomTypeID)
        {
            DeleteRoomTypeResponseDTO deleteRoomTypeResponseDTO = new DeleteRoomTypeResponseDTO();
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand("ToggleRoomTypeActive", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set input parameters of the procedure.
            command.Parameters.AddWithValue("@RoomTypeID", roomTypeID);
            command.Parameters.AddWithValue("@IsActive" , false);
            // set output parameters of the procedure.
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();

            command.Parameters.Add(statusCodeParameter);
            command.Parameters.Add(messageParameter);

            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                deleteRoomTypeResponseDTO.Message = "Room Type Deleted Successfully";
                deleteRoomTypeResponseDTO.IsDeleted = (int)statusCodeParameter.Value == 0;
                return deleteRoomTypeResponseDTO;
            }
            catch (SqlException ex)
            {
                deleteRoomTypeResponseDTO.Message = ex.Message;
                deleteRoomTypeResponseDTO.IsDeleted = false;
                return deleteRoomTypeResponseDTO;
            }
        }

        public async Task<(bool Success, string Message)> ToggleRoomTypeActiveAsync(int RoomTypeID, bool IsActive)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("ToggleRoomTypeActive", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set input parameters of the procedure.
            command.Parameters.AddWithValue("@RoomTypeID", RoomTypeID);
            command.Parameters.AddWithValue("@IsActive", IsActive);
            // set output parameters of the procedure.
            var statusCodeParameter = getStatusCodeParameter();
            var messageParameter = getMessageParameter();

            command.Parameters.Add(statusCodeParameter);
            command.Parameters.Add(messageParameter);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            var ResponseMessage = messageParameter.Value.ToString();
            var success = (int)statusCodeParameter.Value == 0;
            return (success, ResponseMessage);
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
        private SqlParameter getStatusCodeParameter()
        {
            return new SqlParameter
            {
                ParameterName = "@StatusCode",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
        }
    }
}
