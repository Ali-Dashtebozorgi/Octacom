namespace Octacom.API.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }

        public static ApiResponse<T> Ok(T data) => new()
        {
            Success = true,
            Data = data,
            Error = null,
        };

        public static ApiResponse<T> Fail(string error) => new()
        {
            Success = false, 
            Data = default,
            Error = error
        };
    }
}
