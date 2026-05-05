using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace UmbracoSite.Controllers;

[Route("umbraco/backoffice/api/package-export")]
public class PackageExportController : UmbracoAuthorizedApiController
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<PackageExportController> _logger;

    public PackageExportController(
        IWebHostEnvironment webHostEnvironment,
        ILogger<PackageExportController> logger)
    {
        _webHostEnvironment = webHostEnvironment;
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
        var zipPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "frontend-exporter", "marketing-frontend.zip"));

        if (!System.IO.File.Exists(zipPath))
        {
            return NotFound(new { success = false, message = "找不到打包檔，請先執行打包。" });
        }

        var fileName = $"marketing-frontend-{DateTime.Now:yyyyMMdd-HHmmss}.zip";
        var bytes = System.IO.File.ReadAllBytes(zipPath);
        return File(bytes, "application/zip", fileName);
    }
}
