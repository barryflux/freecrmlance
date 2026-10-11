namespace Freecrmlance.Application.Crm;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerDto>> ListDueFollowUpsAsync(DateTime today, CancellationToken cancellationToken = default);
    Task<bool> ScheduleFollowUpAsync(Guid id, DateTime? date, string? note, CancellationToken cancellationToken = default);
    Task<bool> MarkContactedAsync(Guid id, DateTime? nextDate, string? note, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, UpdateCustomerCommand command, CancellationToken cancellationToken = default);
}
