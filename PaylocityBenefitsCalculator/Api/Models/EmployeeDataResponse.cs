using System.Net;

namespace Api.Models
{
    public class EmployeeDataResponse<T>
    {
        public T? EmployeeData { get; set; }
        public Status Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
