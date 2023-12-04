using Maggsoft.Core.Model;
using System;
using System.Collections.Generic;

namespace Maggsoft.Core.Base;

public partial class Result<T> : Result where T : class
{
    private T _data;

    public Result()
    {
        Data = Activator.CreateInstance<T>();
    }

    public T Data
    {
        get => _data;
        set => _data = value;
    }
}

public class Result : IResult
{
    public Result()
    {
        ValidationMessages = [];
    }
    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        ErrorMessage = error;
    }
    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public object ErrorMessage { get; set; }
    public List<string> ValidationMessages { get; set; }
    public int StatusCode { get; set; }
    public DateTime TimeStamp { get; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; }
    public bool IsFailure => !IsSuccess;
    public string ApiVersion { get; set; }
}
