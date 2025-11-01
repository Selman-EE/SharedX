namespace SharedX.Extensions.OperationResult;

/// <summary>
///     Represents the outcome of an operation that can succeed or fail.
///     Use instead of throwing exceptions for expected failures.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public readonly struct OperationResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    private OperationResult(bool isSuccess, T? value, string? errorMessage, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    ///     Creates a successful result with a value.
    /// </summary>
    public static OperationResult<T> Success(T value)
    {
        return new OperationResult<T>(true, value, null);
    }

    /// <summary>
    ///     Creates a failed result with an error message.
    /// </summary>
    public static OperationResult<T> Failure(string errorMessage, string? errorCode = null)
    {
        return new OperationResult<T>(false, default, errorMessage, errorCode);
    }

    /// <summary>
    ///     Pattern matching - executes different logic based on success/failure.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="onSuccess">Function to execute if successful.</param>
    /// <param name="onFailure">Function to execute if failed.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(Value!)
            : onFailure(ErrorMessage!);
    }

    /// <summary>
    ///     Executes action based on success/failure (void version).
    /// </summary>
    public void Match(
        Action<T> onSuccess,
        Action<string> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(ErrorMessage!);
    }

    /// <summary>
    ///     Maps the value if successful, otherwise returns failure.
    /// </summary>
    public OperationResult<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? OperationResult<TNew>.Success(mapper(Value!))
            : OperationResult<TNew>.Failure(ErrorMessage!, ErrorCode);
    }

    /// <summary>
    ///     Chains operations - only executes if current is successful.
    /// </summary>
    public OperationResult<TNew> Bind<TNew>(Func<T, OperationResult<TNew>> binder)
    {
        return IsSuccess
            ? binder(Value!)
            : OperationResult<TNew>.Failure(ErrorMessage!, ErrorCode);
    }

    /// <summary>
    ///     Gets the value or throws an exception if failed.
    /// </summary>
    public T GetValueOrThrow()
    {
        return IsFailure ? throw new InvalidOperationException(ErrorMessage) : Value!;
    }

    /// <summary>
    ///     Gets the value or returns a default if failed.
    /// </summary>
    public T? GetValueOrDefault(T? defaultValue = default)
    {
        return IsSuccess ? Value : defaultValue;
    }
}

/// <summary>
///     Non-generic version for operations without return value.
/// </summary>
public readonly struct OperationResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }
    public string? ErrorCode { get; }

    private OperationResult(bool isSuccess, string? errorMessage, string? errorCode = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
    }

    public static OperationResult Success()
    {
        return new OperationResult(true, null);
    }

    public static OperationResult Failure(string errorMessage, string? errorCode = null)
    {
        return new OperationResult(false, errorMessage, errorCode);
    }

    public void Match(
        Action onSuccess,
        Action<string> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(ErrorMessage!);
    }
}