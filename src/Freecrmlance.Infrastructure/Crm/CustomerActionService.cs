using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Crm;
public sealed class CustomerActionService(FreecrmlanceDbContext db, IWorkspaceContext workspace) : ICustomerActionService
{
    public async Task<IReadOnlyList<CustomerActionDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspace.RequireCurrentWorkspaceIdAsync(cancellationToken);
        if (!await db.Customers.AnyAsync(c => c.Id == customerId && c.WorkspaceId == workspaceId, cancellationToken)) return null;
        var rows = await db.CustomerActions.AsNoTracking().Where(a => a.WorkspaceId == workspaceId && a.CustomerId == customerId)
            .Select(a => new CustomerActionDto(a.Id, a.CustomerId, a.Title, a.Description, a.Status, a.DueDate, a.CreatedAtUtc, a.CompletedAtUtc))
            .ToListAsync(cancellationToken);
        return rows.OrderBy(a => a.Status == CustomerActionStatus.Done ? 1 : 0)
            .ThenBy(a => a.DueDate.HasValue ? 0 : 1).ThenBy(a => a.DueDate).ThenBy(a => a.CreatedAtUtc).ToList();
    }

    public async Task<IReadOnlyList<CustomerActionCountsDto>> GetCountsAsync(IReadOnlyCollection<Guid> customerIds, CancellationToken cancellationToken = default)
    {
        if (customerIds.Count == 0) return [];
        var workspaceId = await workspace.RequireCurrentWorkspaceIdAsync(cancellationToken);
        return await db.CustomerActions.AsNoTracking()
            .Where(a => a.WorkspaceId == workspaceId && customerIds.Contains(a.CustomerId))
            .GroupBy(a => a.CustomerId)
            .Select(g => new CustomerActionCountsDto(g.Key, g.Count(a => a.Status == CustomerActionStatus.Todo), g.Count(a => a.Status == CustomerActionStatus.InProgress)))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> CreateAsync(Guid customerId, SaveCustomerActionCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspace.RequireCurrentWorkspaceIdAsync(cancellationToken);
        if (!await db.Customers.AnyAsync(c => c.Id == customerId && c.WorkspaceId == workspaceId, cancellationToken)) return null;
        var action = new CustomerAction(workspaceId, customerId, command.Title, command.Description, command.DueDate);
        action.SetStatus(command.Status);
        db.CustomerActions.Add(action);
        await db.SaveChangesAsync(cancellationToken);
        return action.Id;
    }

    public async Task<bool> UpdateAsync(Guid customerId, Guid actionId, SaveCustomerActionCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspace.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var action = await db.CustomerActions.SingleOrDefaultAsync(a => a.Id == actionId && a.CustomerId == customerId && a.WorkspaceId == workspaceId, cancellationToken);
        if (action is null) return false;
        action.Update(command.Title, command.Description, command.DueDate);
        action.SetStatus(command.Status);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid customerId, Guid actionId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspace.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var action = await db.CustomerActions.SingleOrDefaultAsync(a => a.Id == actionId && a.CustomerId == customerId && a.WorkspaceId == workspaceId, cancellationToken);
        if (action is null) return false;
        db.CustomerActions.Remove(action);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
