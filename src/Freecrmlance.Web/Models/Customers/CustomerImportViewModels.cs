using Freecrmlance.Application.Crm;

namespace Freecrmlance.Web.Models.Customers;

public sealed class CustomerImportMapViewModel
{
    public required string FileName { get; init; }
    public required IReadOnlyList<CustomerImportColumn> Columns { get; init; }
}

public sealed class CustomerImportPreviewViewModel
{
    public required CustomerImportPreview Preview { get; init; }
    public required IReadOnlyList<CustomerImportMapping> Mappings { get; init; }
}

public sealed class CustomerImportResultViewModel
{
    public required CustomerImportResult Result { get; init; }
}
