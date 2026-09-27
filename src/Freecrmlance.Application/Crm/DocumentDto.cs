namespace Freecrmlance.Application.Crm;

public sealed record DocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    DateTime CreatedAtUtc);
