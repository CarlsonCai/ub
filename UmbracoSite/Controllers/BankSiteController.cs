using Microsoft.AspNetCore.Mvc;
using UmbracoSite.Models;
using UmbracoSite.Services;

namespace UmbracoSite.Controllers;

public class BankSiteController : Controller
{
    private readonly BankSiteRepository _repo;

    public BankSiteController(BankSiteRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        var data = _repo.Get();
        return View("~/Views/Public/Index.cshtml", data);
    }

    [HttpGet("/announcements")]
    public IActionResult Announcements()
    {
        var data = _repo.Get();
        var items = data.Collections.Announcements.Where(x => x.IsPublished).ToList();
        return View("~/Views/Bank/Announcements.cshtml", items);
    }

    [HttpGet("/announcements/{slug}")]
    public IActionResult Announcement(string slug)
    {
        var data = _repo.Get();
        var item = data.Collections.Announcements.FirstOrDefault(x => x.IsPublished && string.Equals(x.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (item is null) return NotFound();
        return View("~/Views/Bank/Announcement.cshtml", item);
    }

    [HttpGet("/articles")]
    public IActionResult Articles()
    {
        var data = _repo.Get();
        var items = data.Collections.Articles.Where(x => x.IsPublished).ToList();
        return View("~/Views/Bank/Articles.cshtml", items);
    }

    [HttpGet("/articles/{slug}")]
    public IActionResult Article(string slug)
    {
        var data = _repo.Get();
        var item = data.Collections.Articles.FirstOrDefault(x => x.IsPublished && string.Equals(x.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (item is null) return NotFound();
        return View("~/Views/Bank/Article.cshtml", item);
    }

    [HttpGet("/products")]
    public IActionResult Products()
    {
        var data = _repo.Get();
        var items = data.Collections.Products.Where(x => x.IsPublished).ToList();
        return View("~/Views/Bank/Products.cshtml", items);
    }

    [HttpGet("/products/{slug}")]
    public IActionResult Product(string slug)
    {
        var data = _repo.Get();
        var item = data.Collections.Products.FirstOrDefault(x => x.IsPublished && string.Equals(x.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (item is null) return NotFound();
        return View("~/Views/Bank/Product.cshtml", item);
    }

    [HttpGet("/promotions")]
    public IActionResult Promotions()
    {
        var data = _repo.Get();
        var items = data.Collections.Promotions.Where(x => x.IsPublished).ToList();
        return View("~/Views/Bank/Promotions.cshtml", items);
    }

    [HttpGet("/promotions/{slug}")]
    public IActionResult Promotion(string slug)
    {
        var data = _repo.Get();
        var item = data.Collections.Promotions.FirstOrDefault(x => x.IsPublished && string.Equals(x.Slug, slug, StringComparison.OrdinalIgnoreCase));
        if (item is null) return NotFound();
        return View("~/Views/Bank/Promotion.cshtml", item);
    }
}

