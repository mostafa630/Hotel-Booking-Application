using System.ComponentModel.DataAnnotations;

namespace Hotel_Booking_Application.DTOs.UserDTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; } // we must hash it beore storing it in the database
    }
}
