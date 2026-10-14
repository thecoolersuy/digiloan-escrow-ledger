namespace DigiLoan.Application.Common.Exceptions;

public class IdentityValidationException : Exception
{
    public IEnumerable<string> Errors { get; }
    public IdentityValidationException(string message, IEnumerable<string> errors) : base(message)
    {
        Errors = errors;
    }
}