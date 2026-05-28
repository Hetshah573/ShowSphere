namespace ShowSphere.Domain.Interfaces;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, string entity, string? entityId = null, string? details = null);
}
