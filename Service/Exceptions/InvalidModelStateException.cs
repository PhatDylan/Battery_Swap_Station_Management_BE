using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Service.Exceptions;

public class InvalidModelStateException: Exception
{
    public object Errors { get; }

    public InvalidModelStateException(ModelStateDictionary modelState)
        : base("Validation failed")
    {
        Errors = modelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => new
            {
                Field = e.Key,
                Errors = e.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
            })
            .ToArray();
    }
}