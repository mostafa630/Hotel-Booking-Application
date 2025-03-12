namespace Hotel_Booking_Application.DTOs.RoomDTOs.RoomTypeDTOs
{
    public class RoomTypeResponseDTO
    {
        public int RoomTypeID { get; set; }
        public string TypeName { get; set; }
        public string AccessibilityFeatures { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
