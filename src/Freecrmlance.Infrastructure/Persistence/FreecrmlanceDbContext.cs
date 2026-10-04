using Freecrmlance.Domain.Audits;
using Freecrmlance.Domain.Billing;
using Freecrmlance.Domain.Crm;
using Freecrmlance.Domain.Platform;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Freecrmlance.Infrastructure.Persistence;
public sealed class FreecrmlanceDbContext(DbContextOptions<FreecrmlanceDbContext> options):IdentityDbContext(options)
{
 public DbSet<Audit> Audits=>Set<Audit>();
 public DbSet<AuditSection> AuditSections=>Set<AuditSection>();
 public DbSet<AuditItem> AuditItems=>Set<AuditItem>();
 public DbSet<AuditItemResponse> AuditItemResponses=>Set<AuditItemResponse>();
 public DbSet<AuditEvidence> AuditEvidence=>Set<AuditEvidence>();
 public DbSet<AuditReportVersion> AuditReportVersions=>Set<AuditReportVersion>();
 public DbSet<AuditReportTransmission> AuditReportTransmissions=>Set<AuditReportTransmission>();
 public DbSet<AuditNumberSequence> AuditNumberSequences=>Set<AuditNumberSequence>();
 public DbSet<AuditTemplate> AuditTemplates=>Set<AuditTemplate>();
 public DbSet<AuditTemplateSection> AuditTemplateSections=>Set<AuditTemplateSection>();
 public DbSet<AuditTemplateItem> AuditTemplateItems=>Set<AuditTemplateItem>();
 public DbSet<Invoice> Invoices=>Set<Invoice>(); public DbSet<InvoiceLine> InvoiceLines=>Set<InvoiceLine>(); public DbSet<InvoiceNumberSequence> InvoiceNumberSequences=>Set<InvoiceNumberSequence>();
 public DbSet<Customer> Customers=>Set<Customer>(); public DbSet<Contact> Contacts=>Set<Contact>(); public DbSet<Document> Documents=>Set<Document>(); public DbSet<DocumentShare> DocumentShares=>Set<DocumentShare>();
 public DbSet<Quote> Quotes=>Set<Quote>(); public DbSet<QuoteLine> QuoteLines=>Set<QuoteLine>(); public DbSet<QuoteShare> QuoteShares=>Set<QuoteShare>();
 public DbSet<Workspace> Workspaces=>Set<Workspace>(); public DbSet<WorkspaceMember> WorkspaceMembers=>Set<WorkspaceMember>();
 protected override void OnModelCreating(ModelBuilder builder){base.OnModelCreating(builder);builder.ApplyConfigurationsFromAssembly(typeof(FreecrmlanceDbContext).Assembly);}
}