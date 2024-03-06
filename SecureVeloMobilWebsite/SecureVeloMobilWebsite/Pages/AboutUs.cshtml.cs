using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SecureVeloMobilWebsite.Pages;

public class AboutUs : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public AboutUs(ILogger<IndexModel> logger)
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