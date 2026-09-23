using Microsoft.AspNetCore.Mvc;

namespace Freecrmlance.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
