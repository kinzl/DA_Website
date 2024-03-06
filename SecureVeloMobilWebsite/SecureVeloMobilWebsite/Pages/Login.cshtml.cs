using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureVeloMobilWebsite.Dto;
using SecureVeloMobilWebsite.Extensions;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Pages;

public class Login : PageModel
{
    public string? ErrorText;
    private readonly ILogger<IndexModel> _logger;
    private VeloMobilContext _db;
    private PasswordEncryption _pe;

    public Login(VeloMobilContext db, ILogger<IndexModel> logger, PasswordEncryption pe)
    {
        _db = db;
        _logger = logger;
        _pe = pe;
    }

    public void OnGet(string? errorText)
    {
        ErrorText = errorText;
    }

    public async Task<IActionResult> OnPostLogin(LoginDto body)
    {
        _logger.LogInformation("OnPostLogin");
        try
        {
            var user = _db.Users.Single(x => x.Username == body.Username);
            if (_pe.VerifyPassword(body.Password, user.PasswordHash))
            {
                var claims = new List<Claim>()
                {
                    new(ClaimTypes.Name, user.Username)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties()
                {
                    IsPersistent = false
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                return new RedirectToPageResult(nameof(Index));
            }
            else
            {
                return new RedirectToPageResult(nameof(Login));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return new RedirectToPageResult(nameof(Login), new { ErrorText = "Password or Username is wrong" });
        }
    }
}