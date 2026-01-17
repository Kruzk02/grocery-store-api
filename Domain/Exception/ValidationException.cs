namespace Domain.Exception;

public class ValidationException(Dictionary<string, string[]> errors) : System.Exception("Validation Error")
{
    public IDictionary<string, string[]> Errors { get; } = errors;
}
