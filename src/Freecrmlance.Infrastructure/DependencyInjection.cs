using Freecrmlance.Application.Audits;
using Freecrmlance.Application.Billing;
using Freecrmlance.Application.Crm;
using Freecrmlance.Application.Platform;
using Freecrmlance.Infrastructure.Audits;
using Freecrmlance.Infrastructure.Billing;
using Freecrmlance.Infrastructure.Crm;
using Freecrmlance.Infrastructure.Documents;
using Freecrmlance.Infrastructure.Persistence;
using Freecrmlance.Infrastructure.Platform;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Freecrmlance.Infrastructure;
public static class DependencyInjection
{
 public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
 {
  var cs=configuration.GetConnectionString("PostgreSQL")??throw new InvalidOperationException("Connection string 'PostgreSQL' is not configured.");
  services.AddDbContext<FreecrmlanceDbContext>(o=>o.UseNpgsql(cs));
  services.AddScoped<IWorkspaceContext,WorkspaceContext>(); services.AddScoped<IWorkspaceSettingsService,WorkspaceSettingsService>();
  services.AddScoped<IAuditService,AuditService>(); services.AddScoped<IAuditFinalizationService,AuditFinalizationService>(); services.AddScoped<IAuditEvidenceService,AuditEvidenceService>(); services.AddScoped<IAuditTemplateService,AuditTemplateService>(); services.AddScoped<IInvoiceService,InvoiceService>(); services.AddScoped<ICustomerService,CustomerService>();
  services.AddScoped<ICustomerImportFileReader,CustomerImportFileReader>(); services.AddScoped<CustomerImportService>(); services.AddScoped<IContactService,ContactService>();
  services.AddScoped<IDocumentService,DocumentService>(); services.AddScoped<IDocumentShareService,DocumentShareService>(); services.AddScoped<IQuoteService,QuoteService>();
  services.AddScoped<IQuoteShareService,QuoteShareService>(); services.AddScoped<IQuotePdfService,QuotePdfService>(); services.AddSingleton<IPdfGenerator,MigraDocPdfGenerator>(); services.AddSingleton<IFileStorage,LocalFileStorage>();
  services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<FreecrmlanceDbContext>().AddDefaultTokenProviders(); return services;
 }
}