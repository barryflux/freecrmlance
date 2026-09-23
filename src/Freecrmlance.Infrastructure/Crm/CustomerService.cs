using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class CustomerService(
    FreecrmlanceDbContext dbContext,
    IWorkspaceContext workspaceContext) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);

        return await dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.WorkspaceId == workspaceId)
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Phone))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var customer = new Customer(workspaceId, command.Name, command.Email, command.Phone);

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }
}
