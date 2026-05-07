using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using UmbracoSite.Services;

namespace UmbracoSite.Controllers;

[Route("umbraco/backoffice/api/package-export")]
public class PackageExportController : UmbracoAuthorizedApiController
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<PackageExportController> _logger;
    private readonly BankSiteRepository _repo;

    public PackageExportController(
        IWebHostEnvironment webHostEnvironment,
        BankSiteRepository repo,
        ILogger<PackageExportController> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _repo = repo;
        _logger = logger;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        var projectRoot = _webHostEnvironment.ContentRootPath;
        var exporterPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "frontend-exporter"));

        if (!Directory.Exists(exporterPath))
        {
            return BadRequest(new { success = false, message = $"找不到匯出工具資料夾：{exporterPath}" });
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c npm run export",
            WorkingDirectory = exporterPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processInfo };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            _logger.LogError("Frontend export failed. stdout: {StdOut} stderr: {StdErr}", stdout, stderr);
            return BadRequest(new
            {
                success = false,
                message = "打包失敗",
                stdout,
                stderr
            });
        }

        return Ok(new
        {
            success = true,
            message = "打包完成",
            downloadUrl = "/umbraco/backoffice/api/package-export/download-latest",
            stdout
        });
    }

    [HttpGet("download-latest")]
    public IActionResult DownloadLatest()
    {
        var projectRoot = _webHostEnvironment.ContentRootPath;
        var zipPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "frontend-exporter", "bank-website.zip"));

        if (!System.IO.File.Exists(zipPath))
        {
            return NotFound(new { success = false, message = "找不到打包檔，請先執行打包。" });
        }

        var fileName = $"bank-website-{DateTime.Now:yyyyMMdd-HHmmss}.zip";
        var bytes = System.IO.File.ReadAllBytes(zipPath);
        return File(bytes, "application/zip", fileName);
    }

    [HttpPost("export-json-raw")]
    public IActionResult ExportJsonRaw()
    {
        return Content(_repo.ReadRawJsonOrEmpty(), "application/json", Encoding.UTF8);
    }

    [HttpPost("export-json")]
    public IActionResult ExportJson()
    {
        var projectRoot = _webHostEnvironment.ContentRootPath;
        var outputPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "frontend-exporter", "dist", "site-data.json"));

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var json = _repo.ReadRawJsonOrEmpty();
        System.IO.File.WriteAllText(outputPath, json, Encoding.UTF8);

        return Ok(new
        {
            success = true,
            message = "JSON 匯出完成",
            downloadUrl = "/umbraco/backoffice/api/package-export/download-json-latest"
        });
    }

    [HttpPost("export-js")]
    public IActionResult ExportJs()
    {
        var projectRoot = _webHostEnvironment.ContentRootPath;
        var outputPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "frontend-exporter", "dist", "site-data.js"));

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        // Read JSON and wrap it into a JS file so frontend can load via <script>.
        // Expose a stable global for consumption without fetch().
        var json = _repo.ReadRawJsonOrEmpty();
        using var doc = JsonDocument.Parse(json);
        var minified = JsonSerializer.Serialize(doc.RootElement);
        var js = $"window.__BANK_SITE_DATA__ = {minified};\n";
        System.IO.File.WriteAllText(outputPath, js, Encoding.UTF8);

        return Ok(new
        {
            success = true,
            message = "JS 匯出完成",
            downloadUrl = "/umbraco/backoffice/api/package-export/download-js-latest"
        });
    }

    [HttpGet("download-json-latest")]
    public IActionResult DownloadJsonLatest()
    {
        var projectRoot = _webHostEnvironment.ContentRootPath;
        var jsonPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "frontend-exporter", "dist", "site-data.json"));

        if (!System.IO.File.Exists(jsonPath))
        {
            return NotFound(new { success = false, message = "找不到 JSON 檔，請先執行匯出。" });
        }

        var fileName = $"site-data-{DateTime.Now:yyyyMMdd-HHmmss}.json";
        var bytes = System.IO.File.ReadAllBytes(jsonPath);
        return File(bytes, "application/json", fileName);
    }

    [HttpGet("download-js-latest")]
    public IActionResult DownloadJsLatest()
    {
        var projectRoot = _webHostEnvironment.ContentRootPath;
        var jsPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "frontend-exporter", "dist", "site-data.js"));

        if (!System.IO.File.Exists(jsPath))
        {
            return NotFound(new { success = false, message = "找不到 JS 檔，請先執行匯出。" });
        }

        var fileName = $"site-data-{DateTime.Now:yyyyMMdd-HHmmss}.js";
        var bytes = System.IO.File.ReadAllBytes(jsPath);
        return File(bytes, "application/javascript", fileName);
    }
}
