namespace Freecrmlance.Application.Crm;
public interface ICustomerActionService
{
    Task<IReadOnlyList<CustomerActionDto>?> ListAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerActionCountsDto>> GetCountsAsync(IReadOnlyCollection<Guid> customerIds, CancellationToken cancellationToken = default);
    Task<Guid?> CreateAsync(Guid customerId, SaveCustomerActionCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid customerId, Guid actionId, SaveCustomerActionCommand command, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid customerId, Guid actionId, CancellationToken cancellationToken = default);
}
