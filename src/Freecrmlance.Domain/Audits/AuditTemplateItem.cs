namespace Freecrmlance.Domain.Audits;

public sealed class AuditTemplateItem
{
    private AuditTemplateItem() { }

    internal AuditTemplateItem(Guid sectionId, string label, AuditResponseType responseType, bool isRequired, int position, string? description, string? options)
    {
        if (sectionId == Guid.Empty) throw new ArgumentException("Section id is required.", nameof(sectionId));
        Id = Guid.NewGuid(); SectionId = sectionId; Position = position;
        Update(label, responseType, isRequired, description, options);
    }

    public Guid Id { get; private set; }
    public Guid SectionId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AuditResponseType ResponseType { get; private set; }
    public bool IsRequired { get; private set; }
    public int Position { get; private set; }
    public string? Options { get; private set; }

    public void Update(string label, AuditResponseType responseType, bool isRequired, string? description, string? options)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Item label is required.", nameof(label));
        var needsOptions = responseType is AuditResponseType.SingleChoice or AuditResponseType.MultipleChoice;
        if (needsOptions && string.IsNullOrWhiteSpace(options)) throw new ArgumentException("Choice items require options.", nameof(options));
        Label = label.Trim(); ResponseType = responseType; IsRequired = isRequired;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Options = needsOptions ? options!.Trim() : null;
    }

    public void SetPosition(int position)
    {
        if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
        Position = position;
    }
}
