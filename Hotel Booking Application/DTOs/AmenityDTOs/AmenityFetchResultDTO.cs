﻿namespace Hotel_Booking_Application.DTOs.AmenityDTOs
{
    public class AmenityFetchResultDTO
    {
        public IEnumerable<AmenityDetailsDTO> Amenities { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
