using System.Net;

namespace be_account.Response
{
    public class CustomResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string? Message { get; set; }
        public object? Result { get; set; }
    }
}