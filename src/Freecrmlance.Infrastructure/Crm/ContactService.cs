using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Freecrmlance.Infrastructure.Crm;

public sealed class ContactService(FreecrmlanceDbContext dbContext, IWorkspaceContext workspaceContext) : IContactService
{
    public async Task<IReadOnlyList<ContactDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var customerExists = await dbContext.Customers.AsNoTracking()
            .AnyAsync(customer => customer.Id == customerId && customer.WorkspaceId == workspaceId, cancellationToken);
        if (!customerExists) return null;

        return await dbContext.Contacts.AsNoTracking()
            .Where(contact => contact.CustomerId == customerId && contact.WorkspaceId == workspaceId)
            .OrderBy(contact => contact.Name)
            .Select(contact => new ContactDto(contact.Id, contact.CustomerId, contact.Name, contact.Email, contact.Phone, contact.Role))
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid?> CreateAsync(Guid customerId, CreateContactCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var customerExists = await dbContext.Customers.AsNoTracking()
            .AnyAsync(customer => customer.Id == customerId && customer.WorkspaceId == workspaceId, cancellationToken);
        if (!customerExists) return null;

        var contact = new Contact(workspaceId, customerId, command.Name, command.Email, command.Phone, command.Role);
        dbContext.Contacts.Add(contact);
        await dbContext.SaveChangesAsync(cancellationToken);
        return contact.Id;
    }
}
