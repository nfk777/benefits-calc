using System.Net;

namespace Api.Models
{
    public class EmployeeDataResponse<T>
    {
        public T? EmployeeData { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
