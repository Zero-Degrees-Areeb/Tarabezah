using System.Collections.Generic;

namespace Tarabezah.Application.Common;

/// <summary>
/// Standardized API response wrapper for all API endpoints
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Custom status code indicating different response types
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// Status message explaining the result
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional error message when there's an error
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// List of detailed error messages when available
    /// </summary>
    public List<string>? ErrorDetails { get; set; }
    
    /// <summary>
    /// The actual response data
    /// </summary>
    public ApiResponseData<T>? Data { get; set; }
    
    /// <summary>
    /// Flag indicating if the request was successful
    /// </summary>
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> Success(T result, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>
        {
            StatusCode = 200,
            Message = message,
            Data = new ApiResponseData<T> { Result = result }
        };
    }

    /// <summary>
    /// Creates a successful response for created resources
    /// </summary>
    public static ApiResponse<T> Created(T result, string message = "Resource created successfully")
    {
        return new ApiResponse<T>
        {
            StatusCode = 201,
            Message = message,
            Data = new ApiResponseData<T> { Result = result }
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> Error(int statusCode, string message, string? errorMessage = null, List<string>? errorDetails = null)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Message = message,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails
        };
    }

    /// <summary>
    /// Creates a not found response
    /// </summary>
    public static ApiResponse<T> NotFound(string message = "Resource not found", string? errorMessage = null)
    {
        return Error(404, message, errorMessage);
    }

    /// <summary>
    /// Creates a bad request response
    /// </summary>
    public static ApiResponse<T> BadRequest(string message = "Bad request", string? errorMessage = null, List<string>? errorDetails = null)
    {
        return Error(400, message, errorMessage, errorDetails);
    }

    /// <summary>
    /// Creates an unauthorized response
    /// </summary>
    public static ApiResponse<T> Unauthorized(string message = "Unauthorized access", string? errorMessage = null)
    {
        return Error(401, message, errorMessage);
    }

    /// <summary>
    /// Creates a server error response
    /// </summary>
    public static ApiResponse<T> ServerError(string message = "Internal server error", string? errorMessage = null)
    {
        return Error(500, message, errorMessage);
    }
}

/// <summary>
/// Container for the actual response data
/// </summary>
public class ApiResponseData<T>
{
    /// <summary>
    /// The actual response result
    /// </summary>
    public T? Result { get; set; }
} 