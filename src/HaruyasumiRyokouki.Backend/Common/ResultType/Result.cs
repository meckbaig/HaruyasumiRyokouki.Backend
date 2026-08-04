using Meckbaig.Result.Base;

namespace HaruyasumiRyokouki.Backend.Common.ResultType;

public readonly struct Result<TValue> : IResult<TValue, StringError>
{
	/// <inheritdoc/>
	public TValue? Value { get; init; }

	/// <inheritdoc/>
	public StringError? Error { get; init; }

	/// <inheritdoc/>
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Value))]
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(Error))]
	public bool IsSuccess => _isSuccess;

	/// <inheritdoc/>
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Error))]
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(Value))]
	public bool IsFailure => !_isSuccess;

	private readonly bool _isSuccess;

	private Result(TValue value)
	{
		_isSuccess = true;
		Value = value;
		Error = default;
	}

	private Result(StringError error)
	{
		_isSuccess = false;
		Value = default;
		Error = error;
	}

	/// <summary>
	/// Conversion operator from <typeparamref name="TValue"/> to a <see cref="Result{TValue, StringError}"/> with success status.
	/// </summary>
	/// <param name="value">Value of the successful result.</param>
	public static implicit operator Result<TValue>(TValue value) => new(value);

	/// <summary>
	/// Conversion operator from <typeparamref name="StringError"/> to a <see cref="Result{TValue, StringError}"/> with failure status.
	/// </summary>
	/// <param name="error">Error value.</param>
	public static implicit operator Result<TValue>(StringError error) => new(error);

	/// <summary>
	/// Creates a successful result containing the given value.
	/// </summary>
	/// <param name="value">Value of the successful result.</param>
	/// <returns>
	/// A new instance of <see cref="Result{TValue,StringError}"/> whose
	/// <see cref="Result{TValue,StringError}.IsSuccess"/> == <see langword="true"/> and
	/// <see cref="Result{TValue,StringError}.Value"/> field is set to <paramref name="value"/>.
	/// </returns>
	public static Result<TValue> Success(TValue value) => new(value);

	/// <summary>
	/// Creates a failed result containing the given error.
	/// </summary>
	/// <param name="error">Error object.</param>
	/// <returns>
	/// A new instance of <see cref="Result{TValue,StringError}"/> whose
	/// <see cref="Result{TValue,StringError}.IsFailure"/> == <see langword="true"/> and
	/// <see cref="Result{TValue,StringError}.Error"/> field is set to <paramref name="error"/>.
	/// </returns>
	public static Result<TValue> Failure(StringError error) => new(error);

	/// <inheritdoc/>
	public TResult Match<TResult>(Func<TValue, TResult> success, Func<StringError, TResult> failure)
	{
		if (IsSuccess)
		{
			return success(Value!);
		}
		return failure(Error!);
	}

	/// <inheritdoc/>
	public async Task<TResult> MatchAsync<TResult>(Func<TValue, Task<TResult>> success, Func<StringError, Task<TResult>> failure)
	{
		if (IsSuccess)
		{
			return await success(Value!);
		}
		return await failure(Error!);
	}

	/// <inheritdoc/>
	public void Switch(Action<TValue> success, Action<StringError> failure)
	{
		if (IsSuccess)
		{
			success(Value!);
		}
		else
		{
			failure(Error!);
		}
	}

	/// <inheritdoc/>
	public async Task SwitchAsync(Func<TValue, Task> success, Func<StringError, Task> failure)
	{
		if (IsSuccess)
		{
			await success(Value!);
		}
		else
		{
			await failure(Error!);
		}
	}

	/// <inheritdoc/>
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Value))]
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(Error))]
	public bool OnSuccess(Action<TValue> success)
	{
		if (IsSuccess)
		{
			success(Value!);
			return true;
		}
		return false;
	}

	/// <inheritdoc/>
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Value))]
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(Error))]
	public async Task<bool> OnSuccessAsync(Func<TValue, Task> success)
	{
		if (IsSuccess)
		{
			await success(Value!);
			return true;
		}
		return false;
	}

	/// <inheritdoc/>
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Error))]
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(Value))]
	public bool OnFailure(Action<StringError> failure)
	{
		if (IsFailure)
		{
			failure(Error!);
			return true;
		}
		return false;
	}

	/// <inheritdoc/>
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Error))]
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(Value))]
	public async Task<bool> OnFailureAsync(Func<StringError, Task> failure)
	{
		if (IsFailure)
		{
			await failure(Error!);
			return true;
		}
		return false;
	}

	/// <inheritdoc cref="IResult.ToString"/>
	public override string ToString() => _isSuccess ? $"Success - {Value}" : $"Failure - {Error}";
}
