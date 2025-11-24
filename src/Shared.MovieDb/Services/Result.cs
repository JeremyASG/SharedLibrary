namespace Shared.MovieDb.Services;

/// <summary>
/// A data envelope that promotes consistency and reusability.
/// Wraps operation results with success/failure status and error messages.
/// </summary>
/// <typeparam name="T">The type of data contained in the result.</typeparam>
public class Result<T>
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// The data returned by the operation (if successful).
    /// </summary>
    public T? Data { get; private set; }

    /// <summary>
    /// The error message (if the operation failed).
    /// </summary>
    public string? Error { get; private set; }

    private Result() { }

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            Error = null
        };
    }

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static Result<T> Failure(string error)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Data = default,
            Error = error
        };
    }
}

/// <summary>
/// A data envelope for operations that don't return data.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// The error message (if the operation failed).
    /// </summary>
    public string? Error { get; private set; }

    private Result() { }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success()
    {
        return new Result
        {
            IsSuccess = true,
            Error = null
        };
    }

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result
        {
            IsSuccess = false,
            Error = error
        };
    }
}
