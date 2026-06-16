namespace FridgeWatch.Application.DTOs;

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string message = "操作成功")
    {
        return new ApiResponse<T> { Code = 200, Message = message, Data = data };
    }

    public static ApiResponse<T> Fail(string message, int code = 400)
    {
        return new ApiResponse<T> { Code = code, Message = message, Data = default };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(string message = "操作成功")
    {
        return new ApiResponse { Code = 200, Message = message, Data = null };
    }

    public static new ApiResponse Fail(string message, int code = 400)
    {
        return new ApiResponse { Code = code, Message = message, Data = null };
    }
}
