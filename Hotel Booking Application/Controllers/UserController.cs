using Azure;
using Hotel_Booking_Application.DTOs.UserDTOs;
using Hotel_Booking_Application.Models;
using Hotel_Booking_Application.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.ComponentModel.Design.Serialization;
using System.Net;

namespace Hotel_Booking_Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserController> _logger;

        public UserController(UserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpPost("AddUser")]
        public async Task<APIResponse<CreateUserResponseDTO>> AddUser(CreateUserDTO createUserDTO)
        {
            _logger.LogInformation("Recieved Request to Add user : {@createUserDTO}", createUserDTO);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid Data in Request body");
                // Return Bad Request
                return new APIResponse<CreateUserResponseDTO>("Invalid Data in Request body", HttpStatusCode.BadRequest);
            }
            
            try
            {
                var response = await _userRepository.AddUserAsync(createUserDTO);
                _logger.LogInformation("CreateUser Response From UserRepository : {@CreateUserResponseDTO}", response);

                if (response.IsCreated)
                {
                    return new APIResponse<CreateUserResponseDTO>(response, response.Message);
                }

                return new APIResponse<CreateUserResponseDTO>(response.Message , HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Adding the user with Email : {@Email}" , createUserDTO.Email);
                return new APIResponse<CreateUserResponseDTO>("Internal Server Error", HttpStatusCode.InternalServerError ,ex.Message);
            }   
        }

        [HttpPost("AssignRole")]
        public async Task<APIResponse<UserRoleResponseDTO>> AssignRole(UserRoleDTO userRoleDTO)
        {
            _logger.LogInformation("Request Received for AssignRole: {@UserRoleDTO}", userRoleDTO);

            if(!ModelState.IsValid)
            {
                _logger.LogError("Invalid Data in Request body");
                // Return Bad Request
                return new APIResponse<UserRoleResponseDTO>("Invalid Data in Request body", HttpStatusCode.BadRequest);
            }

            try
            {
                var response = await _userRepository.AssignRoleToUserAsync(userRoleDTO);
                _logger.LogInformation("AssignRole Response From UserRepository : {@UserRoleResponseDTO}", response);

                if (response.IsAssigned)
                {
                    return new APIResponse<UserRoleResponseDTO>(response, response.Message);
                }
                return new APIResponse<UserRoleResponseDTO>(response.Message , HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleID} to user {UserID}", userRoleDTO.RoleID, userRoleDTO.UserID);
                return new APIResponse<UserRoleResponseDTO>("Role Assigned Failed.", HttpStatusCode.InternalServerError,ex.Message);
            }
        }

        [HttpGet("GetAllUsers")]
        public async Task<APIResponse<List<UserResponseDTO>>> GetAllUsers(bool? isActive = null)
        {
            _logger.LogInformation($"Request Received for GetAllUsers, IsActive: {isActive}");

            try
            {
                var users = await _userRepository.ListAllUsersAsync(isActive);
                return new APIResponse<List<UserResponseDTO>>(users, "Retrieved all Users Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting all users");
                return new APIResponse<List<UserResponseDTO>>("Internal Server Error", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet("GetUserById/{userId}")]
        public async Task<APIResponse<UserResponseDTO>> GetUserById(int userId)
        {
            _logger.LogInformation($"Request Received for GetUserById, UserID: {userId}");
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if(user == null)
                {
                    return new APIResponse<UserResponseDTO>("User Not Found", HttpStatusCode.NotFound);
                }
                return new APIResponse<UserResponseDTO>(user, "Retrieved User Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getting user with ID {UserID}", userId);
                return new APIResponse<UserResponseDTO>("Internal Server Error", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut("UpdateUser/{userId}")]
        public async Task<APIResponse<UpdateUserResponseDTO>> UpdateUser(int userId , [FromBody] UpdateUserDTO updateUserDTO)
        {
            _logger.LogInformation("Request Received for UpdateUser {@UpdateUserDTO}", updateUserDTO);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("UpdateUser Invalid Request Body");
                return new APIResponse<UpdateUserResponseDTO>("Invalid Request Body" , HttpStatusCode.BadRequest);
            }

            if(userId != updateUserDTO.UserID)
            {
                _logger.LogInformation("UpdateUser  : Mismatch userId");
                return new APIResponse<UpdateUserResponseDTO>("Mismatch userId", HttpStatusCode.BadRequest);
            }

            try
            {
                var response = await _userRepository.UpdateUserAsync(updateUserDTO);
                if(response.IsUpdated)
                {
                    return new APIResponse<UpdateUserResponseDTO>(response, response.Message);
                }
                return new APIResponse<UpdateUserResponseDTO>(response.Message,HttpStatusCode.BadRequest);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in updating user with ID {UserID}", userId);
                return new APIResponse<UpdateUserResponseDTO>("Internal Server Error", HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }

        [HttpDelete("DeleteUser/{userId}")]
        public async Task<APIResponse<DeleteUserResponseDTO>> DeleteUser(int userId)
        {
            _logger.LogInformation("Request Received for DeleteUser, Id: {id}",userId);

            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new APIResponse<DeleteUserResponseDTO>("User Not Found", HttpStatusCode.NotFound);
                }

                var response = await _userRepository.DeleteUserAsync(userId);
                if (response.IsDeleted)
                {
                    return new APIResponse<DeleteUserResponseDTO>(response, response.Message);
                }
                return new APIResponse<DeleteUserResponseDTO>(response.Message , HttpStatusCode.BadRequest);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in deleting user with ID {UserID}", userId);
                return new APIResponse<DeleteUserResponseDTO>("Internal Server Error", HttpStatusCode.InternalServerError, ex.Message);
            }


        }

        [HttpPost("Login")]
        public async Task<APIResponse<LoginUserResponseDTO>> LoginUser([FromBody] LoginUserDTO loginUserDTO)
        {
            _logger.LogInformation("Request Received for LoginUser {@LoginUserDTO}", loginUserDTO);
            if (!ModelState.IsValid)
            {
                return new APIResponse<LoginUserResponseDTO>("Invalid Data in the Requrest Body", HttpStatusCode.BadRequest);
            }

            try
            {
                var response = await _userRepository.LoginUserAsync(loginUserDTO);
                if (response.IsLogin)
                {
                    return new APIResponse<LoginUserResponseDTO>(response, response.Message);
                }
                return new APIResponse<LoginUserResponseDTO>(response.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login User with Email {Email}", loginUserDTO.Email);
                return new APIResponse<LoginUserResponseDTO>("Login failed", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost("ToggleActive")]
        public async Task<IActionResult> ToggleActive(int userId, bool isActive)
        {
            try
            {
                var result = await _userRepository.ToggleUserActiveAsync(userId, isActive);
                if (result.Success)
                    return Ok(new { Message = "User activation status updated successfully." });
                else
                    return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for user {UserID}", userId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
