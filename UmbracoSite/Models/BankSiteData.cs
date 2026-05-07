namespace UmbracoSite.Models;

public class SiteData
{
    public int Version { get; set; } = 2;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public SiteSettings SiteSettings { get; set; } = new();
    public HomeConfig Home { get; set; } = HomeConfig.CreateDefault();
    public Collections Collections { get; set; } = Collections.CreateDefault();

    public static SiteData CreateDefault() => new();
}

public class SiteSettings
{
    public string BrandName { get; set; } = "UB 銀行";
    public List<NavItem> NavItems { get; set; } =
    [
        new() { Label = "公告", Href = "/announcements", Sort = 10 },
        new() { Label = "文章", Href = "/articles", Sort = 20 },
        new() { Label = "產品與服務", Href = "/products", Sort = 30 },
        new() { Label = "活動優惠", Href = "/promotions", Sort = 40 }
    ];
    public List<NavItem> FooterLinks { get; set; } =
    [
        new() { Label = "隱私權政策", Href = "/privacy", Sort = 10 },
        new() { Label = "資訊安全", Href = "/security", Sort = 20 }
    ];
    public string ServicePhone { get; set; } = "02-0000-0000";
    public string ServiceHours { get; set; } = "週一至週五 09:00-18:00";
}

public class NavItem
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
    public string Href { get; set; } = "";
    public int Sort { get; set; }
    public bool IsVisible { get; set; } = true;
}

public class HomeConfig
{
    public HomeHero Hero { get; set; } = new()
    {
        Title = "讓金融服務更安心、更便利",
        Subtitle = "信用卡、存匯、貸款、數位帳戶一次到位。內容可由後台上稿與發布。",
        PrimaryCtaLabel = "立即開戶",
        PrimaryCtaHref = "#"
    };

    public HomeSectionConfig Announcements { get; set; } = new() { Mode = HomeSectionMode.Auto, Limit = 5 };
    public HomeSectionConfig Articles { get; set; } = new() { Mode = HomeSectionMode.Auto, Limit = 6 };
    public HomeSectionConfig Products { get; set; } = new() { Mode = HomeSectionMode.Auto, Limit = 3 };
    public HomeSectionConfig Promotions { get; set; } = new() { Mode = HomeSectionMode.Auto, Limit = 3 };

    public static HomeConfig CreateDefault() => new();
}

public class HomeHero
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string PrimaryCtaLabel { get; set; } = "";
    public string PrimaryCtaHref { get; set; } = "";
}

public enum HomeSectionMode
{
    Auto = 0,
    Featured = 1
}

public class HomeSectionConfig
{
    public HomeSectionMode Mode { get; set; } = HomeSectionMode.Auto;
    public int Limit { get; set; } = 6;
    public List<Guid> FeaturedIds { get; set; } = [];
}

public class Collections
{
    public List<Announcement> Announcements { get; set; } = [];
    public List<Article> Articles { get; set; } = [];
    public List<Product> Products { get; set; } = [];
    public List<Promotion> Promotions { get; set; } = [];

    public static Collections CreateDefault() => new()
    {
        Announcements =
        [
            new Announcement
            {
                Title = "系統維護公告（示意）",
                Summary = "本行將於 00:00-02:00 進行例行維護，期間網銀/行動銀行可能短暫中斷。",
                BodyHtml = "<p>維護期間可能影響：網銀登入、轉帳、信用卡查詢等服務。</p><p>造成不便敬請見諒。</p>",
                Date = DateOnly.FromDateTime(DateTime.Today),
                IsPinned = true,
                IsPublished = true,
                Level = "important"
            }
        ],
        Articles =
        [
            new Article
            {
                Title = "新手理財：從記帳到資產配置（示意）",
                Summary = "用簡單的方法建立財務習慣，逐步提高儲蓄率。",
                BodyHtml = "<p>本篇示範文章內頁內容。</p>",
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-3)),
                IsPublished = true,
                Category = "理財入門"
            }
        ],
        Products =
        [
            new Product
            {
                Title = "數位帳戶",
                Summary = "24 小時線上開戶，常用轉帳/查詢一次到位。",
                BodyHtml = "<p>產品介紹（示意）。</p>",
                Sort = 10,
                IsPublished = true
            }
        ],
        Promotions =
        [
            new Promotion
            {
                Title = "新戶首刷禮（示意）",
                Summary = "指定期間完成任務享好禮回饋。",
                BodyHtml = "<p>活動內容（示意）。</p>",
                IsPinned = true,
                IsPublished = true
            }
        ]
    };
}

public abstract class ContentItemBase
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string BodyHtml { get; set; } = "";
    public bool IsPublished { get; set; }
}

public class DatedContentItemBase : ContentItemBase
{
    public DateOnly Date { get; set; }
}

public class Announcement : DatedContentItemBase
{
    public bool IsPinned { get; set; }
    public string Level { get; set; } = "";
}

public class Article : DatedContentItemBase
{
    public string Category { get; set; } = "";
}

public class Product : ContentItemBase
{
    public int Sort { get; set; }
}

public class Promotion : ContentItemBase
{
    public bool IsPinned { get; set; }
}

