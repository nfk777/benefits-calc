namespace Api.Models
{
    public class DataResponse<T>
    {
        public T? Data { get; set; }
        public Status Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
