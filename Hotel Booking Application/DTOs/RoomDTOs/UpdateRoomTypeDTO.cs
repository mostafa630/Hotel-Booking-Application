using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_Application.DTOs.RoomDTOs
{
    public class UpdateRoomTypeDTO
    {
        [Required]
        public int RoomTypeID { get; set; }
        [Required]
        public string TypeName { get; set; }
        public string AccessibilityFeatures { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
