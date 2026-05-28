namespace ShowSphere.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with id '{key}' was not found.") { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Unauthorized") : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "Forbidden") : base(message) { }
}

public class BookingException : Exception
{
    public BookingException(string message) : base(message) { }
}
