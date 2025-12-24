namespace Sale_Saas.Application.Common.Models;

public class Result<T>
{
	public bool Succeeded { get; init; }
	public string ErrorMessage { get; init; }
	public T? Data { get; set; }
	public Result(bool succeeded, string errorMessage)
    {
        Succeeded = succeeded;
		ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T? data)
    {
        var result = new Result<T>(true, string.Empty);
        result.Data = data;

        return result;
    }

    public static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(false, errorMessage);
    }
}
