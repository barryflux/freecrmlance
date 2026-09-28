namespace Freecrmlance.Domain.Billing;

public sealed class Invoice
{
    private readonly List<InvoiceLine> lines = [];
    private Invoice() { }

    public Invoice(Guid workspaceId, Guid customerId, string number, Guid? sourceQuoteId = null)
    {
        if (workspaceId == Guid.Empty) throw new ArgumentException("Workspace is required.", nameof(workspaceId));
        if (customerId == Guid.Empty) throw new ArgumentException("Customer is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Invoice number is required.", nameof(number));
        if (sourceQuoteId == Guid.Empty) throw new ArgumentException("Source quote cannot be empty.", nameof(sourceQuoteId));
        Id = Guid.NewGuid(); WorkspaceId = workspaceId; CustomerId = customerId; Number = number.Trim(); SourceQuoteId = sourceQuoteId;
        Status = InvoiceStatus.Draft; CreatedAtUtc = DateTime.UtcNow; UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? SourceQuoteId { get; private set; }
    public string Number { get; private set; } = string.Empty;
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
    { lines.Add(new InvoiceLine(Id, description, quantity, unitPrice, vatRate)); UpdatedAtUtc = DateTime.UtcNow; }

    public void SetComplianceDetails(DateTime? serviceDate, DateTime? dueDate, string? purchaseOrderReference = null,
        string? vatExemptionMention = null, string? paymentTerms = null, string? earlyPaymentDiscountTerms = null,
        string? latePaymentPenaltyTerms = null, decimal? recoveryCostIndemnity = null)
    {
        if (recoveryCostIndemnity < 0) throw new ArgumentOutOfRangeException(nameof(recoveryCostIndemnity));
        ServiceDate = serviceDate; DueDate = dueDate; PurchaseOrderReference = Normalize(purchaseOrderReference);
        VatExemptionMention = Normalize(vatExemptionMention); PaymentTerms = Normalize(paymentTerms);
        EarlyPaymentDiscountTerms = Normalize(earlyPaymentDiscountTerms); LatePaymentPenaltyTerms = Normalize(latePaymentPenaltyTerms);
        RecoveryCostIndemnity = recoveryCostIndemnity; UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SnapshotSeller(string? legalName, string? legalForm, string? siren, string? siret, string? vatNumber,
        string? addressLine1, string? addressLine2, string? postalCode, string? city, string? countryCode, string? email, string? phone)
    {
        SellerLegalName=legalName; SellerLegalForm=legalForm; SellerSiren=siren; SellerSiret=siret; SellerVatNumber=vatNumber;
        SellerAddressLine1=addressLine1; SellerAddressLine2=addressLine2; SellerPostalCode=postalCode; SellerCity=city;
        SellerCountryCode=countryCode; SellerContactEmail=email; SellerContactPhone=phone;
    }

    public void SnapshotCustomer(string legalName, string? siren, string? siret, string? vatNumber,
        string? addressLine1, string? addressLine2, string? postalCode, string? city, string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(legalName)) throw new ArgumentException("Customer legal name is required.", nameof(legalName));
        CustomerLegalName=legalName.Trim(); CustomerSiren=siren; CustomerSiret=siret; CustomerVatNumber=vatNumber;
        CustomerAddressLine1=addressLine1; CustomerAddressLine2=addressLine2; CustomerPostalCode=postalCode; CustomerCity=city; CustomerCountryCode=countryCode;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
