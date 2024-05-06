namespace APBD8.Exceptions;

public class OrderCompleteException : Exception
{
    public OrderCompleteException(string? message) : base(message)
    {
    }
}