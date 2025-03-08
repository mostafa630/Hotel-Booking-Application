namespace Hotel_Booking_Application.DTOs.UserDTOs
{
    public class CreateUserResponseDTO
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public bool IsCreated { get; set; }
    }
}
