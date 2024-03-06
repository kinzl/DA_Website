using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SecureVeloMobilWebsite.Pages;

public class PrivacyModel : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;

    public PrivacyModel(ILogger<PrivacyModel> logger)
    {
        _logger = logger;
    }

    public IActionResult? OnGet()
    {
        if (HttpContext.User.Identities.ToList().First().Name == null) return new RedirectToPageResult(nameof(Login));
        _logger.LogInformation("User {Name} Signed in", HttpContext.User.Identities.ToList().First().Name);
        return null;
    }
}