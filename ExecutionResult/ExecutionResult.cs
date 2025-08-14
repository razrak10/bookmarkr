namespace bookmarkr.ExecutionResult
{
    internal class ExecutionResult<T>
    {
        public bool IsSuccess { get; }

        public T? Value { get; }

        public string? Message { get; }

        public Exception? Exception { get; }

        public ExecutionResult(T? value)
        {
            IsSuccess = true;
            Value = value;
        }

        public ExecutionResult(string message, Exception? exception = default)
        {
            IsSuccess = false;
            Message = message;
            Exception = exception;
        }

        public static ExecutionResult<T> Success(T value) => new(value);

        public static ExecutionResult<T> Failure(string message, Exception? exception = default) => new(message, exception);

    }
}
