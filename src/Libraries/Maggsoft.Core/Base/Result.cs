using Maggsoft.Core.Model;
using System;
using System.Collections.Generic;

namespace Maggsoft.Core.Base;

public partial class Result<T> : Result where T : class
{
    private T _data;

    public Result() => Data = Activator.CreateInstance<T>();


    protected internal Result(T data, SuccessMessage message)
        : base(true, message) => Data = data;

    protected internal Result(T data, bool isSuccess, Error error)
        : base(isSuccess, error) => Data = data;


    protected internal Result(T data, bool isSuccess, SuccessMessage message)
        : base(isSuccess, message) => Data = data;

    public T Data
    {
        get => _data;
        set => _data = value;
    }

    public static Result<T> Success(T data, SuccessMessage message) => new(data, message);
    public static Result<T> Success(T data) => new(data, SuccessMessage.None);
}

public class Result : IResult
{
    public Result() => ValidationMessages = [];

    protected internal Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Message = error;
    }

    protected internal Result(bool isSuccess, SuccessMessage message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public object Message { get; set; }
    public List<string> ValidationMessages { get; set; }
    public int StatusCode { get; set; }
    public DateTime TimeStamp { get; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; }
    public bool IsFailure => !IsSuccess;
    public string ApiVersion { get; set; }

    public static Result Success() => new(true, SuccessMessage.None);
    public static Result Success(SuccessMessage message) => new(true, message);
    public static Result Failure(Error error) => new(false, error);
}
