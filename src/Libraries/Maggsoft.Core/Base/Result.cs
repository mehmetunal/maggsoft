using System;
using System.Collections.Generic;
using Maggsoft.Core.Model;

namespace Maggsoft.Core.Base;

public partial class Result<T> : Result
{
    private T _data;

    public Result()
    {
        Data = default!;
    }

    private Result(T data)
    {
        Data = data;
    }

    protected internal Result(T data, string message)
        : base(true, message) => Data = data;

    protected internal Result(T data, bool isSuccess, List<string> errors)
        : base(isSuccess, errors) => Data = data;

    protected internal Result(T data, bool isSuccess, string message)
        : base(isSuccess, message) => Data = data;

    public T Data
    {
        get => _data;
        set => _data = value;
    }

    public static implicit operator Result<T>(T data) => Success(data);
    public static Result<T> Success(T data, string message) => new(data, true, message);
    public static Result<T> Success(T data) => new(data, true, string.Empty);
    public static Result<T> Failure(List<string> errors) => new(default!, false, errors);
    public static Result<T> Failure(string error) => new(default!, false, [error]);

}

public class Result : IResult
{
    public Result() => Errors = [];

    protected internal Result(bool isSuccess, List<string> errors)
    {
        if (isSuccess && errors.Count > 0 ||
            !isSuccess && errors.Count == 0)
        {
            throw new ArgumentException("Invalid errors", nameof(errors));
        }

        IsSuccess = isSuccess;
        Errors = errors;
    }

    protected internal Result(bool isSuccess, string message)
        => (IsSuccess, Message) = (isSuccess, message);


    public string Message { get; set; }
    public List<string> Errors { get; set; }
    public bool IsSuccess { get; set; }

    public static Result Success() => new(true, string.Empty);
    public static Result Success(string message) => new(true, message);
    public static Result Failure(List<string> errors) => new(false, errors);
}
