using Freecrmlance.Application.Platform;
using Freecrmlance.Infrastructure;
using Freecrmlance.Web.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
CultureInfo.DefaultThreadCurrentCulture = frenchCulture;
CultureInfo.DefaultThreadCurrentUICulture = frenchCulture;

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<BetaUserProvisioner>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (args.Contains("--create-beta-user", StringComparer.OrdinalIgnoreCase))
{
    var emailIndex = Array.FindIndex(args, value => value.Equals("--email", StringComparison.OrdinalIgnoreCase));
    if (emailIndex < 0 || emailIndex + 1 >= args.Length)
    {
        Console.Error.WriteLine("Usage : dotnet Freecrmlance.Web.dll --create-beta-user --email personne@example.com [--workspace \"Nom espace\"]");
        return;
    }

    var workspaceIndex = Array.FindIndex(args, value => value.Equals("--workspace", StringComparison.OrdinalIgnoreCase));
    var workspaceName = workspaceIndex >= 0 && workspaceIndex + 1 < args.Length
        ? args[workspaceIndex + 1]
        : "Mon espace";

    await using var scope = app.Services.CreateAsyncScope();
    var provisioner = scope.ServiceProvider.GetRequiredService<BetaUserProvisioner>();
    Environment.ExitCode = await provisioner.CreateAsync(args[emailIndex + 1], workspaceName);
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program;
