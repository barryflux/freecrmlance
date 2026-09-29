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
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

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
