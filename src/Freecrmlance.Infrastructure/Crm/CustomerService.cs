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
        return await dbContext.Customers.AsNoTracking()
            .Where(customer => customer.WorkspaceId == workspaceId)
            .OrderBy(customer => customer.Name)
            .Select(customer => new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Phone, customer.Type, customer.Siren, customer.Siret, customer.VatNumber,
                customer.AddressLine1, customer.AddressLine2, customer.PostalCode, customer.City, customer.CountryCode,
                customer.BillingAddressLine1, customer.BillingAddressLine2, customer.BillingPostalCode, customer.BillingCity, customer.BillingCountryCode))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        return await dbContext.Customers.AsNoTracking()
            .Where(customer => customer.Id == id && customer.WorkspaceId == workspaceId)
            .Select(customer => new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Phone, customer.Type, customer.Siren, customer.Siret, customer.VatNumber,
                customer.AddressLine1, customer.AddressLine2, customer.PostalCode, customer.City, customer.CountryCode,
                customer.BillingAddressLine1, customer.BillingAddressLine2, customer.BillingPostalCode, customer.BillingCity, customer.BillingCountryCode))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var customer = new Customer(workspaceId, command.Name, command.Email, command.Phone);
        if (command.Type is not null)
            customer.SetBillingIdentity(command.Type.Value, command.AddressLine1!, command.PostalCode!, command.City!, command.CountryCode!,
                command.Siren, command.Siret, command.VatNumber, command.AddressLine2,
                command.BillingAddressLine1, command.BillingAddressLine2, command.BillingPostalCode, command.BillingCity, command.BillingCountryCode);
        dbContext.Customers.Add(customer);

        foreach (var contact in command.Contacts ?? [])
        {
            dbContext.Contacts.Add(new Contact(
                workspaceId,
                customer.Id,
                contact.Name,
                contact.Email,
                contact.Phone,
                contact.Role));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var workspaceId = await workspaceContext.RequireCurrentWorkspaceIdAsync(cancellationToken);
        var customer = await dbContext.Customers
            .SingleOrDefaultAsync(customer => customer.Id == id && customer.WorkspaceId == workspaceId, cancellationToken);

        if (customer is null)
            return false;

        customer.Update(command.Name, command.Email, command.Phone);
        if (command.Type is not null)
            customer.SetBillingIdentity(command.Type.Value, command.AddressLine1!, command.PostalCode!, command.City!, command.CountryCode!,
                command.Siren, command.Siret, command.VatNumber, command.AddressLine2,
                command.BillingAddressLine1, command.BillingAddressLine2, command.BillingPostalCode, command.BillingCity, command.BillingCountryCode);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
