using System.Net;

namespace Hotel_Booking_Application.Models
{
    public class APIResponse<T>
    {
        public bool Sucess { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public object Error { get; set; }

        // Successful Response 
        public APIResponse(T data, string message = "", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Sucess = true;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Error = null;
        }

        // Error Response
        public APIResponse(string message, HttpStatusCode statusCode, object error = null)
        {
            Sucess = false;
            StatusCode = statusCode;
            Message = message;
            Data = default(T);
            Error = error;
        }


    }
}
