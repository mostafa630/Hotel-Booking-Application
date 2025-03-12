using Microsoft.AspNetCore.Mvc;
using System.Net;
using Hotel_Booking_Application.Repositories;
using Hotel_Booking_Application.DTOs.RoomDTOs;
using Hotel_Booking_Application.Models;


namespace Hotel_Booking_Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly RoomRepository _roomRepository;
        private readonly ILogger<RoomController> _logger;
        public RoomController(RoomRepository roomRepository, ILogger<RoomController> logger)
        {
            _roomRepository = roomRepository;
            _logger = logger;
        }

        [HttpGet("GetAllRooms")]
        public async Task<APIResponse<List<RoomDetailsResponseDTO>>> GetAllRooms([FromQuery] GetAllRoomsDTO request)
        {
            _logger.LogInformation("Request Received for CreateRoomType: {@GetAllRoomsRequestDTO}", request);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body");
                return new APIResponse<List<RoomDetailsResponseDTO>>("Invalid Data in the Query String" , HttpStatusCode.BadRequest);
            }
            try
            {
                var rooms = await _roomRepository.GetAllRoomsAsync(request);
                return new APIResponse<List<RoomDetailsResponseDTO>>(rooms, "Retrieved all Room Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Retriving all Room");
                return new APIResponse<List<RoomDetailsResponseDTO>>("Internal server error" , HttpStatusCode.InternalServerError , ex.Message);
            }
        }

        [HttpGet("GetRoomById/{id}")]
        public async Task<APIResponse<RoomDetailsResponseDTO>> GetRoomById(int id)
        {
            _logger.LogInformation($"Request Received for GetRoomById, id: {id}");
            try
            {
                var response = await _roomRepository.GetRoomByIdAsync(id);
                if (response == null)
                {
                    return new APIResponse<RoomDetailsResponseDTO>("Room ID not found" , HttpStatusCode.NotFound);
                }
                return new APIResponse<RoomDetailsResponseDTO>(response, "Room fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Room by ID {id}", id);
                return new APIResponse<RoomDetailsResponseDTO>("Error fetching Room.", HttpStatusCode.BadRequest, ex.Message);
            }
        }

        [HttpPost("CreateRoom")]
        public async Task<APIResponse<CreateRoomResponseDTO>> CreateRoom([FromBody] CreateRoomDTO request)
        {
            _logger.LogInformation("Request Received for CreateRoom: {@CreateRoomRequestDTO}", request);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body");
                return new APIResponse<CreateRoomResponseDTO>("Invalid Data in the Requrest Body" ,HttpStatusCode.BadRequest);
            }
            try
            {
                var response = await _roomRepository.CreateRoomAsync(request);
                _logger.LogInformation("CreateRoom Response From Repository: {@CreateRoomResponseDTO}", response);
                if (response.IsCreated)
                {
                    return new APIResponse<CreateRoomResponseDTO>(response, response.Message);
                }
                return new APIResponse<CreateRoomResponseDTO>(response.Message ,HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new Room");
                return new APIResponse<CreateRoomResponseDTO>("Room Creation Failed.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut("UpdatRoom/{id}")]
        public async Task<APIResponse<UpdateRoomResponseDTO>> UpdateRoom(int id, [FromBody] UpdateRoomDTO request)
        {
            _logger.LogInformation("Request Received for UpdateRoom {@UpdateRoomRequestDTO}", request);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("UpdateRoom Invalid Request Body");
                return new APIResponse<UpdateRoomResponseDTO>("Invalid Request Body" , HttpStatusCode.BadRequest);
            }
            if (id != request.RoomID)
            {
                _logger.LogInformation("UpdateRoom Mismatched Room ID");
                return new APIResponse<UpdateRoomResponseDTO>("Mismatched Room ID." , HttpStatusCode.BadRequest);
            }
            try
            {
                var response = await _roomRepository.UpdateRoomAsync(request);
                if (response.IsUpdated)
                {
                    return new APIResponse<UpdateRoomResponseDTO>(response, response.Message);
                }
                return new APIResponse<UpdateRoomResponseDTO>(response.Message , HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Updating Room {id}", id);
                return new APIResponse<UpdateRoomResponseDTO>("Update Room Failed.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete("DeleteRoom/{id}")]
        public async Task<APIResponse<DeleteRoomResponseDTO>> DeleteRoom(int id)
        {
            _logger.LogInformation($"Request Received for DeleteRoom, id: {id}");
            try
            {
                var room = await _roomRepository.GetRoomByIdAsync(id);
                if (room == null)
                {
                    return new APIResponse<DeleteRoomResponseDTO>("Room not found.", HttpStatusCode.NotFound);
                }
                var response = await _roomRepository.DeleteRoomAsync(id);
                if (response.IsDeleted)
                {
                    return new APIResponse<DeleteRoomResponseDTO>(response, response.Message);
                }
                return new APIResponse<DeleteRoomResponseDTO>(response.Message , HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Room {id}", id);
                return new APIResponse<DeleteRoomResponseDTO>("Internal server error: " , HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
