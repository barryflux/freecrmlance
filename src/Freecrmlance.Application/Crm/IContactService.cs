namespace Freecrmlance.Application.Crm;

public interface IContactService
{
    Task<IReadOnlyList<ContactDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Guid?> CreateAsync(Guid customerId, CreateContactCommand command, CancellationToken cancellationToken = default);
}
