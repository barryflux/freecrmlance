namespace Freecrmlance.Application.Crm;

public sealed record DocumentDownloadDto(
    string FileName,
    string ContentType,
    Stream Content);
