namespace Freecrmlance.Domain.Audits;

public sealed class AuditTemplateSection
{
    private readonly List<AuditTemplateItem> _items = [];
    private AuditTemplateSection() { }

    internal AuditTemplateSection(Guid auditTemplateId, string title, int position, string? description)
    {
        if (auditTemplateId == Guid.Empty) throw new ArgumentException("Template id is required.", nameof(auditTemplateId));
        Id = Guid.NewGuid(); AuditTemplateId = auditTemplateId; Position = position;
        Update(title, description);
    }

    public Guid Id { get; private set; }
    public Guid AuditTemplateId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Position { get; private set; }
    public IReadOnlyCollection<AuditTemplateItem> Items => _items;

    public void Update(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Section title is required.", nameof(title));
        Title = title.Trim(); Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public AuditTemplateItem AddItem(string label, AuditResponseType responseType, bool isRequired = false, string? description = null, string? options = null)
    {
        var item = new AuditTemplateItem(Id, label, responseType, isRequired, _items.Count, description, options);
        _items.Add(item); return item;
    }

    public void SetPosition(int position)
    {
        if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
        Position = position;
    }
}
