using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_Application.DTOs.RoomDTOs.RoomTypeDTOs
{
    public class CreateRoomTypeDTO
    {
        [Required]
        public string TypeName { get; set; }
        public string AccessibilityFeatures { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
