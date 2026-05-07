using System.Text;
using System.Text.Json;
using UmbracoSite.Models;

namespace UmbracoSite.Services;

public class BankSiteRepository
{
    private readonly IWebHostEnvironment _env;
    private readonly object _lock = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new UmbracoSite.Json.LenientDateOnlyJsonConverter() }
    };

    public BankSiteRepository(IWebHostEnvironment env)
    {
        _env = env;
    }

    public string GetDataPath() =>
        Path.GetFullPath(Path.Combine(_env.ContentRootPath, "App_Data", "bank-site", "site-data.json"));

    public SiteData Get()
    {
        var path = GetDataPath();
        lock (_lock)
        {
            if (!File.Exists(path))
            {
                return SiteData.CreateDefault();
            }

            var json = File.ReadAllText(path, Encoding.UTF8);
            // v1 migration: old shape { announcements, articles, version, updatedAt }
            if (json.Contains("\"announcements\"", StringComparison.OrdinalIgnoreCase) &&
                !json.Contains("\"collections\"", StringComparison.OrdinalIgnoreCase))
            {
                var migrated = MigrateV1ToV2(json);
                Save(migrated);
                return migrated;
            }

            var data = JsonSerializer.Deserialize<SiteData>(json, JsonOptions);

            return data ?? SiteData.CreateDefault();
        }
    }

    public void Save(SiteData input)
    {
        input ??= new SiteData();
        input.Version = 2;
        input.UpdatedAt = DateTimeOffset.UtcNow;
        Normalize(input);

        var path = GetDataPath();
        lock (_lock)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(input, JsonOptions);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
    }

    public string ReadRawJsonOrEmpty()
    {
        var path = GetDataPath();
        lock (_lock)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }

            return JsonSerializer.Serialize(new
            {
                version = 2,
                updatedAt = DateTimeOffset.UtcNow,
                siteSettings = new { },
                home = new { },
                collections = new { announcements = Array.Empty<object>(), articles = Array.Empty<object>(), products = Array.Empty<object>(), promotions = Array.Empty<object>() }
            }, JsonOptions);
        }
    }

    private static void Normalize(SiteData data)
    {
        data.SiteSettings ??= new SiteSettings();
        data.SiteSettings.NavItems ??= [];
        data.SiteSettings.FooterLinks ??= [];

        foreach (var n in data.SiteSettings.NavItems)
        {
            n.Id = n.Id == Guid.Empty ? Guid.NewGuid() : n.Id;
            n.Label ??= "";
            n.Href ??= "";
        }
        foreach (var n in data.SiteSettings.FooterLinks)
        {
            n.Id = n.Id == Guid.Empty ? Guid.NewGuid() : n.Id;
            n.Label ??= "";
            n.Href ??= "";
        }

        data.Home ??= HomeConfig.CreateDefault();
        data.Home.Announcements ??= new HomeSectionConfig();
        data.Home.Articles ??= new HomeSectionConfig();
        data.Home.Products ??= new HomeSectionConfig();
        data.Home.Promotions ??= new HomeSectionConfig();

        data.Collections ??= Collections.CreateDefault();
        data.Collections.Announcements ??= [];
        data.Collections.Articles ??= [];
        data.Collections.Products ??= [];
        data.Collections.Promotions ??= [];

        var usedAnnouncementSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Collections.Announcements)
        {
            item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
            item.Title ??= "";
            item.Summary ??= "";
            item.BodyHtml ??= "";
            item.Level ??= "";
            item.Date = item.Date == default ? DateOnly.FromDateTime(DateTime.Today) : item.Date;

            var baseSlug = Slugify(string.IsNullOrWhiteSpace(item.Slug) ? item.Title : item.Slug);
            item.Slug = EnsureUniqueSlug(baseSlug, usedAnnouncementSlugs);
        }

        var usedArticleSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Collections.Articles)
        {
            item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
            item.Title ??= "";
            item.Summary ??= "";
            item.BodyHtml ??= "";
            item.Category ??= "";
            item.Date = item.Date == default ? DateOnly.FromDateTime(DateTime.Today) : item.Date;

            var baseSlug = Slugify(string.IsNullOrWhiteSpace(item.Slug) ? item.Title : item.Slug);
            item.Slug = EnsureUniqueSlug(baseSlug, usedArticleSlugs);
        }

        var usedProductSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Collections.Products)
        {
            item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
            item.Title ??= "";
            item.Summary ??= "";
            item.BodyHtml ??= "";
            var baseSlug = Slugify(string.IsNullOrWhiteSpace(item.Slug) ? item.Title : item.Slug);
            item.Slug = EnsureUniqueSlug(baseSlug, usedProductSlugs);
        }

        var usedPromotionSlugs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Collections.Promotions)
        {
            item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
            item.Title ??= "";
            item.Summary ??= "";
            item.BodyHtml ??= "";
            var baseSlug = Slugify(string.IsNullOrWhiteSpace(item.Slug) ? item.Title : item.Slug);
            item.Slug = EnsureUniqueSlug(baseSlug, usedPromotionSlugs);
        }

        data.Collections.Announcements = data.Collections.Announcements
            .OrderByDescending(x => x.IsPinned)
            .ThenByDescending(x => x.Date)
            .ThenBy(x => x.Title)
            .ToList();

        data.Collections.Articles = data.Collections.Articles
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Title)
            .ToList();

        data.Collections.Products = data.Collections.Products
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Title)
            .ToList();

        data.Collections.Promotions = data.Collections.Promotions
            .OrderByDescending(x => x.IsPinned)
            .ThenBy(x => x.Title)
            .ToList();
    }

    private static SiteData MigrateV1ToV2(string v1Json)
    {
        using var doc = JsonDocument.Parse(v1Json);
        var root = doc.RootElement;

        var data = new SiteData
        {
            Version = 2,
            UpdatedAt = DateTimeOffset.UtcNow,
            SiteSettings = new SiteSettings(),
            Home = HomeConfig.CreateDefault(),
            Collections = Collections.CreateDefault()
        };

        if (root.TryGetProperty("updatedAt", out var updatedAt) && updatedAt.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(updatedAt.GetString(), out var dto))
        {
            data.UpdatedAt = dto;
        }

        if (root.TryGetProperty("announcements", out var ann) && ann.ValueKind == JsonValueKind.Array)
        {
            data.Collections.Announcements = JsonSerializer.Deserialize<List<Announcement>>(ann.GetRawText(), JsonOptions) ?? [];
        }
        if (root.TryGetProperty("articles", out var art) && art.ValueKind == JsonValueKind.Array)
        {
            data.Collections.Articles = JsonSerializer.Deserialize<List<Article>>(art.GetRawText(), JsonOptions) ?? [];
        }

        return data;
    }

    private static string EnsureUniqueSlug(string baseSlug, HashSet<string> used)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? Guid.NewGuid().ToString("n")[..8] : baseSlug;
        if (used.Add(slug)) return slug;

        for (var i = 2; i < 9999; i++)
        {
            var candidate = $"{slug}-{i}";
            if (used.Add(candidate)) return candidate;
        }

        var fallback = $"{slug}-{Guid.NewGuid():N}"[..Math.Min(32, slug.Length + 1 + 32)];
        used.Add(fallback);
        return fallback;
    }

    private static string Slugify(string? input)
    {
        input ??= "";
        var sb = new StringBuilder();
        var prevDash = false;

        foreach (var ch in input.Trim().ToLowerInvariant())
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
            {
                sb.Append(ch);
                prevDash = false;
                continue;
            }

            if (!prevDash)
            {
                sb.Append('-');
                prevDash = true;
            }
        }

        var slug = sb.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("n")[..8] : slug;
    }
}

