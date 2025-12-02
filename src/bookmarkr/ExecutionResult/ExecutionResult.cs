namespace bookmarkr.ExecutionResult;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
/// <remarks>
/// This class implements the Result pattern for functional error handling,
/// allowing operations to return success values or failure information
/// without throwing exceptions for expected failure cases.
/// <para>
/// Example usage:
/// <code>
/// var result = await repository.GetByIdAsync(id);
/// if (result.IsSuccess)
/// {
///     Console.WriteLine(result.Value.Name);
/// }
/// else if (result.IsNotFound)
/// {
///     Console.WriteLine("Entity not found");
/// }
/// else
/// {
///     Console.WriteLine($"Error: {result.Message}");
/// }
/// </code>
/// </para>
/// </remarks>
public class ExecutionResult<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed because the requested entity was not found.
    /// </summary>
    public bool IsNotFound { get; }

    /// <summary>
    /// Gets the result value. Only valid when <see cref="IsSuccess"/> is <c>true</c>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the error message. Only valid when <see cref="IsSuccess"/> is <c>false</c>.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets the exception that caused the failure, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Initializes a new successful <see cref="ExecutionResult{T}"/>.
    /// </summary>
    /// <param name="value">The success value.</param>
    public ExecutionResult(T? value)
    {
        IsSuccess = true;
        IsNotFound = false;
        Value = value;
    }

    /// <summary>
    /// Initializes a new failed <see cref="ExecutionResult{T}"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The optional exception that caused the failure.</param>
    /// <param name="isNotFound">Indicates whether this is a "not found" failure.</param>
    public ExecutionResult(string message, Exception? exception = default, bool isNotFound = false)
    {
        IsSuccess = false;
        IsNotFound = isNotFound;
        Message = message;
        Exception = exception;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful <see cref="ExecutionResult{T}"/>.</returns>
    public static ExecutionResult<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The optional exception that caused the failure.</param>
    /// <returns>A failed <see cref="ExecutionResult{T}"/>.</returns>
    public static ExecutionResult<T> Failure(string message, Exception? exception = default) => new(message, exception);

    /// <summary>
    /// Creates a "not found" result indicating the requested entity does not exist.
    /// </summary>
    /// <param name="message">The not found message.</param>
    /// <returns>A "not found" <see cref="ExecutionResult{T}"/>.</returns>
    public static ExecutionResult<T> NotFound(string message) => new(message, isNotFound: true);

    /// <summary>
    /// Converts this failure result to a failure result of a different type.
    /// </summary>
    /// <typeparam name="TResult">The target result type.</typeparam>
    /// <returns>A new failure result with the same error information.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called on a successful result.</exception>
    public ExecutionResult<TResult> ToFailure<TResult>()
    {
        if (IsSuccess)
        {
            throw new InvalidOperationException("Cannot convert successful ExecutionResult to failure ExecutionResult.");
        }

        return new ExecutionResult<TResult>(Message!, Exception, IsNotFound);
    }
}

