namespace Freecrmlance.Application.Crm;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, UpdateCustomerCommand command, CancellationToken cancellationToken = default);
}
