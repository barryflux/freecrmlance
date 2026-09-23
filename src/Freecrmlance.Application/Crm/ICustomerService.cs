namespace Freecrmlance.Application.Crm;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default);
}
