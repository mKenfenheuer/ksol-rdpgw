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
            .Include(r => r.RDPResource)
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
        sb.AppendLine("screen mode id:i:2");
        sb.AppendLine("use multimon:i:0");
        sb.AppendLine("desktopwidth:i:1920");
        sb.AppendLine("desktopheight:i:1080");
        sb.AppendLine("session bpp:i:32");
        sb.AppendLine("winposstr:s:0,1,0,0,800,600");
        sb.AppendLine("compression:i:1");
        sb.AppendLine("keyboardhook:i:2");
        sb.AppendLine("audiocapturemode:i:0");
        sb.AppendLine("videoplaybackmode:i:1");
        sb.AppendLine("connection type:i:7");
        sb.AppendLine("networkautodetect:i:1");
        sb.AppendLine("bandwidthautodetect:i:1");
        sb.AppendLine("displayconnectionbar:i:1");
        sb.AppendLine("enableworkspacereconnect:i:0");
        sb.AppendLine("remoteappmousemoveinject:i:1");
        sb.AppendLine("disable wallpaper:i:0");
        sb.AppendLine("allow font smoothing:i:0");
        sb.AppendLine("allow desktop composition:i:0");
        sb.AppendLine("disable full window drag:i:1");
        sb.AppendLine("disable menu anims:i:1");
        sb.AppendLine("disable themes:i:0");
        sb.AppendLine("disable cursor setting:i:0");
        sb.AppendLine("bitmapcachepersistenable:i:1");
        sb.AppendLine("full address:s:" + host);
        sb.AppendLine("audiomode:i:0");
        sb.AppendLine("redirectprinters:i:1");
        sb.AppendLine("redirectlocation:i:0");
        sb.AppendLine("redirectcomports:i:0");
        sb.AppendLine("redirectsmartcards:i:1");
        sb.AppendLine("redirectwebauthn:i:1");
        sb.AppendLine("redirectclipboard:i:1");
        sb.AppendLine("redirectposdevices:i:0");
        sb.AppendLine("drivestoredirect:s:");
        sb.AppendLine("autoreconnection enabled:i:1");
        sb.AppendLine("authentication level:i:2");
        sb.AppendLine("prompt for credentials:i:0");
        sb.AppendLine("negotiate security layer:i:1");
        sb.AppendLine("remoteapplicationmode:i:0");
        sb.AppendLine("alternate shell:s:");
        sb.AppendLine("shell working directory:s:");
        sb.AppendLine("gatewayhostname:s:" + host);
        sb.AppendLine("gatewayusagemethod:i:2");
        sb.AppendLine("gatewaycredentialssource:i:3");
        sb.AppendLine("gatewayprofileusagemethod:i:1");
        sb.AppendLine("promptcredentialonce:i:0");
        sb.AppendLine("gatewaybrokeringtype:i:0");
        sb.AppendLine("use redirection server name:i:0");
        sb.AppendLine("rdgiskdcproxy:i:0");
        sb.AppendLine("kdcproxyname:s:");
        sb.AppendLine("enablerdsaadauth:i:0");
        sb.AppendLine("username:s:" + _userManager.GetUserName(User));

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
