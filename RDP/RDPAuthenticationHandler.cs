using System.Text;
using Microsoft.AspNetCore.Identity;
using RDPGW.AspNetCore;

namespace KSol.RDPGateway.RDP;

public class RDPAuthenticationHandler : IRDPGWAuthenticationHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RDPAuthenticationHandler> _logger;

    public RDPAuthenticationHandler(IServiceScopeFactory scopeFactory, ILogger<RDPAuthenticationHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<RDPGWAuthenticationResult> HandleBasicAuth(string auth)
    {
        var challenge = Encoding.UTF8.GetString(Convert.FromBase64String(auth));
        var split = challenge.Split(":");

        var userName = split[0];
        var userPassword = split[1];

        _logger.LogInformation($"Basic auth user: {userName}");

        var signInManager = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<SignInManager<IdentityUser>>();

        var result = await signInManager.PasswordSignInAsync(userName, userPassword, false, false);

        if (result.Succeeded)
        {
            var user = await signInManager.UserManager.FindByNameAsync(userName);
            if (user != null)
            {
                _logger.LogInformation($"User {userName} authenticated successfully");
                return RDPGWAuthenticationResult.Success(user.Id);
            }
        }

        _logger.LogWarning($"User {userName} authentication failed");
        return RDPGWAuthenticationResult.Failed();
    }

    public Task<RDPGWAuthenticationResult> HandleDigestAuth(string auth)
    {
        return Task.FromResult(RDPGWAuthenticationResult.Failed());
    }

    public Task<RDPGWAuthenticationResult> HandleNegotiateAuth(string auth)
    {
        return Task.FromResult(RDPGWAuthenticationResult.Failed());
    }
}
