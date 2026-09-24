namespace Freecrmlance.Application.Crm;

public interface IContactService
{
    Task<IReadOnlyList<ContactDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<ContactDto?> GetAsync(Guid customerId, Guid contactId, CancellationToken cancellationToken = default);
    Task<Guid?> CreateAsync(Guid customerId, CreateContactCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid customerId, Guid contactId, UpdateContactCommand command, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid customerId, Guid contactId, CancellationToken cancellationToken = default);
}
