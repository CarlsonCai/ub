using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Umbraco.Cms.Web.BackOffice.Controllers;
using UmbracoSite.Services;

namespace UmbracoSite.Controllers;

[Route("umbraco/backoffice/api/bank-content")]
public class BankContentController : UmbracoAuthorizedApiController
{
    private readonly BankSiteRepository _repo;

    public BankContentController(BankSiteRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("get")]
    public IActionResult Get()
    {
        var data = _repo.Get();
        // Umbraco backoffice AngularJS expects camelCase.
        // Force camelCase serialization here to avoid binder/option drift.
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            Converters = { new UmbracoSite.Json.LenientDateOnlyJsonConverter() }
        });

        return Content(json, "application/json");
    }

    [HttpPost("save")]
    public IActionResult Save([FromBody] UmbracoSite.Models.SiteData input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "資料格式不正確（日期請用 YYYY-MM-DD）" });
        }

        _repo.Save(input);
        return Ok(new { success = true, message = "已儲存" });
    }
}

