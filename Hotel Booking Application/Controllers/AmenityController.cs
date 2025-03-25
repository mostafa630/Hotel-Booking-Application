using Hotel_Booking_Application.DTOs.AmenityDTOs;
using Hotel_Booking_Application.Models;
using Hotel_Booking_Application.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Hotel_Booking_Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AmenityController : ControllerBase
    {
        private readonly AmenityRepository _amenityRepository;
        private readonly ILogger<AmenityController> _logger;
        public AmenityController(AmenityRepository amenityRepository, ILogger<AmenityController> logger)
        {
            _amenityRepository = amenityRepository;
            _logger = logger;
        }
        [HttpGet("Fetch")]
        public async Task<APIResponse<AmenityFetchResultDTO>> FetchAmenities(bool? isActive = null)
        {
            try
            {
                var response = await _amenityRepository.FetchAmenitiesAsync(isActive);
                if (response.IsSuccess)
                {
                    return new APIResponse<AmenityFetchResultDTO>(response, "Retrieved all Room Amenity Successfully.");
                }
                return new APIResponse<AmenityFetchResultDTO>(response.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching amenities.");
                return new APIResponse<AmenityFetchResultDTO>("An error occurred while processing your request.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet("Fetch/{id}")]
        public async Task<APIResponse<AmenityDetailsDTO>> FetchAmenityById(int id)
        {
            try
            {
                var response = await _amenityRepository.FetchAmenityByIdAsync(id);
                if (response != null)
                {
                    return new APIResponse<AmenityDetailsDTO>(response, "Retrieved Room Amenity Successfully.");
                }
                return new APIResponse<AmenityDetailsDTO>("Amenity ID not found.", HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching amenity by ID.");
                return new APIResponse<AmenityDetailsDTO>("An error occurred while processing your request.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost("Add")]
        public async Task<APIResponse<AmenityInsertResponseDTO>> AddAmenity([FromBody] AmenityInsertDTO amenity)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<AmenityInsertResponseDTO>("Invalid Data in the Requrest Body", HttpStatusCode.BadRequest);
                }
                var response = await _amenityRepository.AddAmenityAsync(amenity);
                if (response.IsCreated)
                {
                    return new APIResponse<AmenityInsertResponseDTO>(response, response.Message);
                }
                return new APIResponse<AmenityInsertResponseDTO>(response.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding amenity.");
                return new APIResponse<AmenityInsertResponseDTO>("Amenity Creation Failed.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut("Update/{id}")]
        public async Task<APIResponse<AmenityUpdateResponseDTO>> UpdateAmenity(int id, [FromBody] AmenityUpdateDTO amenity)
        {
            try
            {
                if (id != amenity.AmenityID)
                {
                    _logger.LogInformation("UpdateRoom Mismatched Amenity ID");
                    return new APIResponse<AmenityUpdateResponseDTO>("Mismatched Amenity ID.", HttpStatusCode.BadRequest);
                }
                if (!ModelState.IsValid)
                {
                    return new APIResponse<AmenityUpdateResponseDTO>("Invalid Request Body", HttpStatusCode.BadRequest);
                }
                var response = await _amenityRepository.UpdateAmenityAsync(amenity);
                if (response.IsUpdated)
                {
                    return new APIResponse<AmenityUpdateResponseDTO>(response, response.Message);
                }
                return new APIResponse<AmenityUpdateResponseDTO>(response.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating amenity.");
                return new APIResponse<AmenityUpdateResponseDTO>("An error occurred while processing your request.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<APIResponse<AmenityDeleteResponseDTO>> DeleteAmenity(int id)
        {
            try
            {
                var amenity = await _amenityRepository.FetchAmenityByIdAsync(id);
                if (amenity == null)
                {
                    return new APIResponse<AmenityDeleteResponseDTO>("Amenity not found.", HttpStatusCode.NotFound);
                }
                var response = await _amenityRepository.DeleteAmenityAsync(id);
                if (response.IsDeleted)
                {
                    return new APIResponse<AmenityDeleteResponseDTO>(response, response.Message);
                }
                return new APIResponse<AmenityDeleteResponseDTO>(response.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting amenity.");
                return new APIResponse<AmenityDeleteResponseDTO>("An error occurred while processing your request.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost("BulkInsert")]
        public async Task<APIResponse<AmenityBulkOperationResultDTO>> BulkInsertAmenities(List<AmenityInsertDTO> amenities)
        {
            try
            {
                var response = await _amenityRepository.BulkInsertAmenitiesAsync(amenities);
                if (response.IsSuccess)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>(response, response.Message);
                }
                return new APIResponse<AmenityBulkOperationResultDTO>(response.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk inserting amenities.");
                return new APIResponse<AmenityBulkOperationResultDTO>("An error occurred while processing your request.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost("BulkUpdate")]
        public async Task<APIResponse<AmenityBulkOperationResultDTO>> BulkUpdateAmenities(List<AmenityUpdateDTO> amenities)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>("Invalid Data in the Request Body", HttpStatusCode.BadRequest);
                }
                var response = await _amenityRepository.BulkUpdateAmenitiesAsync(amenities);
                if (response.IsSuccess)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>(response, response.Message);
                }
                return new APIResponse<AmenityBulkOperationResultDTO>(response.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk updating amenities.");
                return new APIResponse<AmenityBulkOperationResultDTO>("An error occurred while processing your request.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPut("BulkUpdateStatus")]
        public async Task<APIResponse<AmenityBulkOperationResultDTO>> BulkUpdateAmenityStatus(List<AmenityStatusDTO> amenityStatuses)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>("Invalid Data in the Request Body", HttpStatusCode.BadRequest);
                }
                var response = await _amenityRepository.BulkUpdateAmenityStatusAsync(amenityStatuses);
                if (response.IsSuccess)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>(response, response.Message);
                }
                return new APIResponse<AmenityBulkOperationResultDTO>(response.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk updating amenity status.");
                return new APIResponse<AmenityBulkOperationResultDTO>("An error occurred while processing your request.", HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
