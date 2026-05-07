using Microsoft.AspNetCore.Mvc;
using UmbracoSite.Services;

namespace UmbracoSite.Controllers;

public class SiteDataController : Controller
{
    private readonly BankSiteRepository _repo;

    public SiteDataController(BankSiteRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("/site-data.json")]
    public IActionResult GetSiteData()
    {
        return Content(_repo.ReadRawJsonOrEmpty(), "application/json");
    }
}

