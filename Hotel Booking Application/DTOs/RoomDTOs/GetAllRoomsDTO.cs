using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_Application.DTOs.RoomDTOs
{
    public class GetAllRoomsDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Room Type ID must be a positive integer.")]
        public int? RoomTypeID { get; set; }

        [RegularExpression("(Available|Under Maintenance|Occupied|All)", ErrorMessage = "Invalid status. Valid statuses are 'Available', 'Under Maintenance', 'Occupied', or 'All' for no filter.")]
        public string? Status { get; set; }
    }
}
