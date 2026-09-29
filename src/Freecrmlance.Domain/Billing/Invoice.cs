namespace Freecrmlance.Domain.Billing;

public sealed class Invoice
{
    private readonly List<InvoiceLine> lines = [];
    private Invoice() { }

    public Invoice(Guid workspaceId, Guid customerId, string draftReference, Guid? sourceQuoteId = null)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace is required.", nameof(workspaceId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(draftReference)) throw new ArgumentException("Draft reference is required.", nameof(draftReference));
        if (sourceQuoteId == Guid.Empty) throw new ArgumentException("Source quote cannot be empty.", nameof(sourceQuoteId));
        Id = Guid.NewGuid(); WorkspaceId = workspaceId; CustomerId = customerId; DraftReference = draftReference.Trim(); SourceQuoteId = sourceQuoteId;
        Status = InvoiceStatus.Draft; CreatedAtUtc = DateTime.UtcNow; UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? SourceQuoteId { get; private set; }
    public string DraftReference { get; private set; } = string.Empty;
    public string? Number { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime? IssueDate { get; private set; }
    public DateTime? ServiceDate { get; private set; }
    public DateTime? DueDate { get; private set; }
    public string? PurchaseOrderReference { get; private set; }
    public string? VatExemptionMention { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? EarlyPaymentDiscountTerms { get; private set; }
    public string? LatePaymentPenaltyTerms { get; private set; }
    public decimal? RecoveryCostIndemnity { get; private set; }

    public string? SellerLegalName { get; private set; }
    public string? SellerLegalForm { get; private set; }
    public string? SellerSiren { get; private set; }
    public string? SellerSiret { get; private set; }
    public string? SellerVatNumber { get; private set; }
    public string? SellerAddressLine1 { get; private set; }
    public string? SellerAddressLine2 { get; private set; }
    public string? SellerPostalCode { get; private set; }
    public string? SellerCity { get; private set; }
    public string? SellerCountryCode { get; private set; }
    public string? SellerContactEmail { get; private set; }
    public string? SellerContactPhone { get; private set; }

    public string? CustomerLegalName { get; private set; }
    public string? CustomerSiren { get; private set; }
    public string? CustomerSiret { get; private set; }
    public string? CustomerVatNumber { get; private set; }
    public string? CustomerAddressLine1 { get; private set; }
    public string? CustomerAddressLine2 { get; private set; }
    public string? CustomerPostalCode { get; private set; }
    public string? CustomerCity { get; private set; }
    public string? CustomerCountryCode { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<InvoiceLine> Lines => lines;
    public decimal TotalExcludingTax => lines.Sum(line => line.TotalExcludingTax);
    public decimal TotalVat => lines.Sum(line => line.VatAmount);
    public decimal TotalIncludingTax => lines.Sum(line => line.TotalIncludingTax);
    public decimal Total => TotalIncludingTax;

    public void AddLine(string description, decimal quantity, decimal unitPrice, decimal vatRate = 0m)
    {
        EnsureDraft();
        lines.Add(new InvoiceLine(Id, description, quantity, unitPrice, vatRate));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateDraft(string draftReference, DateTime? serviceDate, DateTime? dueDate, string? purchaseOrderReference,
        string? vatExemptionMention, string? paymentTerms, string? earlyPaymentDiscountTerms,
        string? latePaymentPenaltyTerms, decimal? recoveryCostIndemnity,
        IEnumerable<(string Description, decimal Quantity, decimal UnitPrice, decimal VatRate)> replacementLines)
    {
        EnsureDraft();
        if (string.IsNullOrWhiteSpace(draftReference)) throw new ArgumentException("Draft reference is required.", nameof(draftReference));
        if (replacementLines is null) throw new ArgumentNullException(nameof(replacementLines));

        var validatedLines = replacementLines
            .Select(line => new InvoiceLine(Id, line.Description, line.Quantity, line.UnitPrice, line.VatRate))
            .ToList();
        if (validatedLines.Count == 0) throw new ArgumentException("At least one invoice line is required.", nameof(replacementLines));
        if (recoveryCostIndemnity < 0) throw new ArgumentOutOfRangeException(nameof(recoveryCostIndemnity));

        DraftReference = draftReference.Trim();
        ServiceDate = NormalizeUtcDate(serviceDate);
        DueDate = NormalizeUtcDate(dueDate);
        PurchaseOrderReference = Normalize(purchaseOrderReference);
        VatExemptionMention = Normalize(vatExemptionMention);
        PaymentTerms = Normalize(paymentTerms);
        EarlyPaymentDiscountTerms = Normalize(earlyPaymentDiscountTerms);
        LatePaymentPenaltyTerms = Normalize(latePaymentPenaltyTerms);
        RecoveryCostIndemnity = recoveryCostIndemnity;
        lines.Clear();
        lines.AddRange(validatedLines);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Issue(string definitiveNumber, DateTime issuedAtUtc)
    {
        EnsureDraft();
        if (string.IsNullOrWhiteSpace(definitiveNumber)) throw new ArgumentException("Definitive invoice number is required.", nameof(definitiveNumber));
        ValidateForIssuance();

        Number = definitiveNumber.Trim();
        IssueDate = issuedAtUtc.Kind == DateTimeKind.Utc ? issuedAtUtc : issuedAtUtc.ToUniversalTime();
        Status = InvoiceStatus.Issued;
        UpdatedAtUtc = IssueDate.Value;
    }

    public void SetComplianceDetails(DateTime? serviceDate, DateTime? dueDate, string? purchaseOrderReference = null,
        string? vatExemptionMention = null, string? paymentTerms = null, string? earlyPaymentDiscountTerms = null,
        string? latePaymentPenaltyTerms = null, decimal? recoveryCostIndemnity = null)
    {
        EnsureDraft();
        if (recoveryCostIndemnity < 0) throw new ArgumentOutOfRangeException(nameof(recoveryCostIndemnity));
        ServiceDate = NormalizeUtcDate(serviceDate); DueDate = NormalizeUtcDate(dueDate); PurchaseOrderReference = Normalize(purchaseOrderReference);
        VatExemptionMention = Normalize(vatExemptionMention); PaymentTerms = Normalize(paymentTerms);
        EarlyPaymentDiscountTerms = Normalize(earlyPaymentDiscountTerms); LatePaymentPenaltyTerms = Normalize(latePaymentPenaltyTerms);
        RecoveryCostIndemnity = recoveryCostIndemnity; UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SnapshotSeller(string? legalName, string? legalForm, string? siren, string? siret, string? vatNumber,
        string? addressLine1, string? addressLine2, string? postalCode, string? city, string? countryCode, string? email, string? phone)
    {
        EnsureDraft();
        SellerLegalName=legalName; SellerLegalForm=legalForm; SellerSiren=siren; SellerSiret=siret; SellerVatNumber=vatNumber;
        SellerAddressLine1=addressLine1; SellerAddressLine2=addressLine2; SellerPostalCode=postalCode; SellerCity=city;
        SellerCountryCode=countryCode; SellerContactEmail=email; SellerContactPhone=phone;
    }

    public void SnapshotCustomer(string legalName, string? siren, string? siret, string? vatNumber,
        string? addressLine1, string? addressLine2, string? postalCode, string? city, string? countryCode)
    {
        EnsureDraft();
        if (string.IsNullOrWhiteSpace(legalName)) throw new ArgumentException("Customer legal name is required.", nameof(legalName));
        CustomerLegalName=legalName.Trim(); CustomerSiren=siren; CustomerSiret=siret; CustomerVatNumber=vatNumber;
        CustomerAddressLine1=addressLine1; CustomerAddressLine2=addressLine2; CustomerPostalCode=postalCode; CustomerCity=city; CustomerCountryCode=countryCode;
    }

    private void ValidateForIssuance()
    {
        if (lines.Count == 0) throw new InvalidOperationException("An invoice must contain at least one line.");
        if (string.IsNullOrWhiteSpace(SellerLegalName) || string.IsNullOrWhiteSpace(SellerAddressLine1) ||
            string.IsNullOrWhiteSpace(SellerPostalCode) || string.IsNullOrWhiteSpace(SellerCity) || string.IsNullOrWhiteSpace(SellerCountryCode))
            throw new InvalidOperationException("Seller legal identity is incomplete.");
        if (string.IsNullOrWhiteSpace(CustomerLegalName) || string.IsNullOrWhiteSpace(CustomerAddressLine1) ||
            string.IsNullOrWhiteSpace(CustomerPostalCode) || string.IsNullOrWhiteSpace(CustomerCity) || string.IsNullOrWhiteSpace(CustomerCountryCode))
            throw new InvalidOperationException("Customer billing identity is incomplete.");
        if (ServiceDate is null) throw new InvalidOperationException("Service date is required.");
        if (DueDate is null && string.IsNullOrWhiteSpace(PaymentTerms)) throw new InvalidOperationException("Payment due date or payment terms are required.");
        if (lines.Any(line => line.VatRate == 0m) && string.IsNullOrWhiteSpace(VatExemptionMention))
            throw new InvalidOperationException("A VAT exemption mention is required when a line has a zero VAT rate.");
    }

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft) throw new InvalidOperationException("Issued invoices cannot be modified.");
    }

    private static DateTime? NormalizeUtcDate(DateTime? value)
    {
        if (value is null) return null;
        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
