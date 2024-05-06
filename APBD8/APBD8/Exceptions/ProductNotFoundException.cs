namespace APBD8.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(string? message) : base(message)
    {
    }
}