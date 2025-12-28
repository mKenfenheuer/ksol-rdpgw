using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KSol.RDPGateway.Data;
using KSol.RDPGateway.Models;

namespace KSol.RDPGateway.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return View(new List<RDPResource>());
        }

        // Get resources the user has access to
        var resources = await _context.RDPResourceUserAuthorizations
            .Where(r => r.UserId == userId)
            .Select(r => r.RDPResource)
            .ToListAsync();

        return View(resources ?? new List<RDPResource>());
    }

    public async Task<IActionResult> DownloadRdpFile(string id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        // Check if user has access to this resource
        var authorization = await _context.RDPResourceUserAuthorizations
            .FirstOrDefaultAsync(r => r.UserId == userId && r.RDPResourceId == id);

        if (authorization?.RDPResource == null)
        {
            return NotFound();
        }

        var resource = authorization.RDPResource;
        var host = Request.Host.Host;

        // Generate RDP file content
        var rdpContent = GenerateRdpFileContent(resource, host);
        var fileBytes = Encoding.UTF8.GetBytes(rdpContent);

        return File(fileBytes, "application/x-rdp", $"{resource.Name ?? resource.ResourceIdentifier}.rdp");
    }

    private string GenerateRdpFileContent(RDPResource resource, string host)
    {
        var sb = new StringBuilder();
        sb.AppendLine("auto connect:i:1");
        sb.AppendLine("full address:s:" + host);
        sb.AppendLine("username:s:");
        sb.AppendLine("domain:s:");
        sb.AppendLine("password 51:b:0");
        sb.AppendLine("prompt for credentials on client:i:1");
        sb.AppendLine("negotiate security layer:i:1");
        sb.AppendLine("allow font smoothing:i:1");
        sb.AppendLine("compression:i:1");
        sb.AppendLine("keyboardhook:i:2");
        sb.AppendLine("audiomode:i:0");
        sb.AppendLine("redirectdrives:i:0");
        sb.AppendLine("redirectprinters:i:0");
        sb.AppendLine("redirectcomports:i:0");
        sb.AppendLine("redirectsmartcards:i:0");
        sb.AppendLine("displayconnectionbar:i:1");
        sb.AppendLine("username:s:" + resource.ResourceIdentifier);
        sb.AppendLine("remoteapplicationmode:i:0");
        sb.AppendLine("shell working directory:s:");
        sb.AppendLine("disable wallpaper:i:0");
        sb.AppendLine("disable full window drag:i:0");
        sb.AppendLine("disable menu anims:i:0");
        sb.AppendLine("disable themes:i:0");
        sb.AppendLine("alternate shell:s:");
        sb.AppendLine("shell:s:");
        sb.AppendLine("gatewayhostname:s:");
        sb.AppendLine("session bpp:i:32");
        sb.AppendLine("videoplaybackmode:i:1");

        return sb.ToString();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
