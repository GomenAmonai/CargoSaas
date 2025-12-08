namespace Cargo.Core.Exceptions;

/// <summary>
/// Базовый класс для всех бизнес-исключений приложения
/// </summary>
public abstract class CargoException : Exception
{
    public int StatusCode { get; }

    protected CargoException(string message, int statusCode = 400) 
        : base(message)
    {
        StatusCode = statusCode;
    }

    protected CargoException(string message, Exception innerException, int statusCode = 400) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}

/// <summary>
/// Исключение при ошибке валидации данных
/// </summary>
public class ValidationException : CargoException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) 
        : base(message, 400)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.", 400)
    {
        Errors = errors;
    }
}

/// <summary>
/// Исключение когда ресурс не найден
/// </summary>
public class NotFoundException : CargoException
{
    public NotFoundException(string resourceName, object key) 
        : base($"{resourceName} with key '{key}' was not found.", 404)
    {
    }

    public NotFoundException(string message) 
        : base(message, 404)
    {
    }
}

/// <summary>
/// Исключение при попытке неавторизованного доступа
/// </summary>
public class UnauthorizedException : CargoException
{
    public UnauthorizedException(string message = "Unauthorized access.") 
        : base(message, 401)
    {
    }
}

/// <summary>
/// Исключение при попытке доступа к запрещенному ресурсу
/// </summary>
public class ForbiddenException : CargoException
{
    public ForbiddenException(string message = "Access forbidden.") 
        : base(message, 403)
    {
    }
}

/// <summary>
/// Исключение при конфликте (например, дубликат уникального поля)
/// </summary>
public class ConflictException : CargoException
{
    public ConflictException(string message) 
        : base(message, 409)
    {
    }

    public ConflictException(string resourceName, string field, object value) 
        : base($"{resourceName} with {field} '{value}' already exists.", 409)
    {
    }
}

/// <summary>
/// Исключение при бизнес-логической ошибке
/// </summary>
public class BusinessException : CargoException
{
    public BusinessException(string message) 
        : base(message, 422) // Unprocessable Entity
    {
    }
}
