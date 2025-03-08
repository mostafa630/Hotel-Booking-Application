using Hotel_Booking_Application.Connection;
using Hotel_Booking_Application.DTOs.UserDTOs;
using Hotel_Booking_Application.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Immutable;
using System.Data;

namespace Hotel_Booking_Application.Repositories
{
    public class UserRepository(SqlConnectionFactory _connectionFactory)
    {
        public async Task<CreateUserResponseDTO> AddUserAsync(CreateUserDTO user)
        {
            var createUserResponseDTO = new CreateUserResponseDTO();

            using var connection = _connectionFactory.CreateConnection();
            //create command object and set data to be passed to the stored procedure
            using var command = new SqlCommand("AddUser", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.Password);
            command.Parameters.AddWithValue("@createdBy", "System");

            // set the out parameters of the procedure
            var userIdParameter = new SqlParameter
            {
                ParameterName = "@UserID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var errorMessageParameter = getErrorMessageParameter();
            // add the out parameters to the command
            command.Parameters.Add(userIdParameter);
            command.Parameters.Add(errorMessageParameter);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            var userId = (int)userIdParameter.Value;

            if (userId != -1)
            {
                createUserResponseDTO.UserId = userId;
                createUserResponseDTO.IsCreated = true;
                createUserResponseDTO.Message = "User Created Successfully";
                return createUserResponseDTO;
            }

            // if the user is not created
            var errorMessage = errorMessageParameter.Value?.ToString();
            createUserResponseDTO.Message = errorMessage ?? "thre is a probrlm happend when creating a user";
            createUserResponseDTO.IsCreated = false;

            return createUserResponseDTO;
        }

        public async Task<UserRoleResponseDTO> AssignRoleToUserAsync(UserRoleDTO userRole)
        {
            UserRoleResponseDTO userRoleResponseDTO = new UserRoleResponseDTO();

            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand("AssignUserRole", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure
            command.Parameters.AddWithValue("@UserID", userRole.UserID);
            command.Parameters.AddWithValue("@RoleID", userRole.RoleID);

            // set the out parameters of the procedure
            var errorMessageParameter = getErrorMessageParameter();
            // add the out parameters to the command
            command.Parameters.Add(errorMessageParameter);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            var errorMessage = errorMessageParameter.Value?.ToString();

            if (string.IsNullOrEmpty(errorMessage)) // no error message so operation was done
            {
                userRoleResponseDTO.Message = "Role Assigned Successfully";
                userRoleResponseDTO.IsAssigned = true;
            }
            else
            {
                userRoleResponseDTO.Message = errorMessage;
                userRoleResponseDTO.IsAssigned = false;
            }
            return userRoleResponseDTO;
        }

        public async Task<List<UserResponseDTO>> ListAllUsersAsync(bool ? isActive)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("ListAllUsers", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure
            //if the Active parameter is not null then add it to the command
            //and if it is null send it as DBNull.Value
            command.Parameters.AddWithValue("@IsActice", (object)isActive?? DBNull.Value);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            
            var users = new List<UserResponseDTO>();
            while (reader.Read())
            {
                var user = new UserResponseDTO
                {
                    UserID = reader.GetInt32("UserID"),
                    Email = reader.GetString("Email"),
                    IsActive = reader.GetBoolean("IsActive"),
                    RoleID = reader.GetInt32("RoleID"),
                    // we use the extension method here beciase the value can be null if this is the first login
                    LastLogin = reader.GetValeByColumn<DateTime?>("LastLogin"),
                };
                users.Add(user);
            }
            return users;
        }

        public async Task<UserResponseDTO> GetUserByIdAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("GetUserById", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure
            command.Parameters.AddWithValue("@UserID", userId);
            // set the out parameters of the procedure
            var errorMessageParameter = getErrorMessageParameter();
            // add the out parameters to the command
            command.Parameters.Add(errorMessageParameter);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (!reader.Read())
            {
                return null;
            }

            var user  = new UserResponseDTO
            {
                UserID = reader.GetInt32("UserID"),
                Email = reader.GetString("Email"),
                IsActive = reader.GetBoolean("IsActive"),
                RoleID = reader.GetInt32("RoleID"),
                LastLogin = reader.GetValeByColumn<DateTime?>("LastLogin"),
            };
            return user;
        }

        public async Task<UpdateUserResponseDTO> UpdateUserAsync(UpdateUserDTO user)
        {
            var updatedUserResponseDTO = new UpdateUserResponseDTO()
            {
                UserId = user.UserID // because the id is not updated
            };

            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("UpdateUserInformation", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure
            command.Parameters.AddWithValue("@UserID", user.UserID);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.Password);
            command.Parameters.AddWithValue("@ModifiedBy", "System");

            var errorMessageParameter = getErrorMessageParameter();
            command.Parameters.Add(errorMessageParameter);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            var message = errorMessageParameter.Value?.ToString();
            if(string.IsNullOrEmpty(message)) // no error message so the operation was done
            {
                updatedUserResponseDTO.Message = "User info Updated Successfully";
                updatedUserResponseDTO.IsUpdated = true;
            }
            else
            {
                updatedUserResponseDTO.Message = message;
                updatedUserResponseDTO.IsUpdated = false;
            }
            return updatedUserResponseDTO;

        }

        public async Task<DeleteUserResponseDTO> DeleteUserAsync(int userId)
        {
            var deltedUserResponseDTO = new DeleteUserResponseDTO();

            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("ToggleUserActive", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure
            command.Parameters.AddWithValue("@UserID", userId);
            command.Parameters.AddWithValue("@IsActive", false);
            // set the out parameters of the procedure
            var errorMessageParameter = getErrorMessageParameter();
            // add the out parameters to the command
            command.Parameters.Add(errorMessageParameter);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            var message = errorMessageParameter.Value?.ToString();
            if (string.IsNullOrEmpty(message))
            {
                deltedUserResponseDTO.Message = "User Deleted Successfully";
                deltedUserResponseDTO.IsDeleted = true;
            }
            else
            {
                deltedUserResponseDTO.Message = message;
                deltedUserResponseDTO.IsDeleted = false;
            }
            return deltedUserResponseDTO;
        }

        public async Task<LoginUserResponseDTO> LoginUserAsync(LoginUserDTO login)
        {
            var loginUserResponseDTO = new LoginUserResponseDTO();
            using var connection = _connectionFactory.CreateConnection();

            using var command = new SqlCommand("LoginUser", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure
            command.Parameters.AddWithValue("@Email", login.Email);
            command.Parameters.AddWithValue("@PasswordHash", login.Password);
            // set the out parameters of the procedure
            var userIdParameter = new SqlParameter
            {
                ParameterName = "@UserID",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
            var errorMessageParameter = getErrorMessageParameter();
            // add the out parameters to the command
            command.Parameters.Add(userIdParameter);
            command.Parameters.Add(errorMessageParameter);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            var success = userIdParameter.Value != DBNull.Value && (int)userIdParameter.Value > 0;

            if (success)
            {
                var userId = (int)userIdParameter.Value;
                loginUserResponseDTO.UserId = userId;
                loginUserResponseDTO.IsLogin = true;
                loginUserResponseDTO.Message = "Login Success";
                return loginUserResponseDTO;
            }
            else
            {
                var errorMessage = errorMessageParameter.Value?.ToString();
                loginUserResponseDTO.Message = errorMessage ?? "Error happens while logining";
                loginUserResponseDTO.IsLogin = false;
                return loginUserResponseDTO;
            }
        }

        public async Task<(bool Success, string Message)> ToggleUserActiveAsync(int userId, bool isActive)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("ToggleUserActive", connection);
            command.CommandType = CommandType.StoredProcedure;
            // set the in parameters of the procedure
            command.Parameters.AddWithValue("@UserID", userId);
            command.Parameters.AddWithValue("@IsActive", isActive);
            // set the out parameters of the procedure
            var errorMessageParameter = getErrorMessageParameter();
            // add the out parameters to the command
            command.Parameters.Add(errorMessageParameter);

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();

            var message = errorMessageParameter.Value?.ToString();
            var success = string.IsNullOrEmpty(message);

            return (success, message);
        }

        private SqlParameter getErrorMessageParameter()
        {
            return new SqlParameter
            {
                ParameterName = "@ErrorMessage",
                SqlDbType = SqlDbType.NVarChar,
                Size = 255,
                Direction = ParameterDirection.Output
            };
        }
    }
}