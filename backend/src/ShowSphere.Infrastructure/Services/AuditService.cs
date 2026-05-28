using ShowSphere.Domain.Entities;
using ShowSphere.Domain.Interfaces;
using ShowSphere.Infrastructure.Data;

namespace ShowSphere.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(Guid? userId, string action, string entity, string? entityId = null, string? details = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Details = details
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }
}
