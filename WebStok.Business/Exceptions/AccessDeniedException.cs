namespace WebStok.Business.Exceptions;

public class AccessDeniedException : Exception
{
    public AccessDeniedException(string message)
        : base(message)
    {
    }
}