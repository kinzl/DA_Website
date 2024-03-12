using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureVeloMobilWebsite.Dto;
using SecureVeloMobilWebsite.wwwroot.Extensions;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private VeloMobilContext _db;
    public List<CourseDto> Courses;
    public int SelectedCourseIndex;
    public int SelectedCourseId;
    public List<DetailPositionDto> DetailPositions;
    public int SelectedFilterDateIndex;
    public CourseDto SelectedCourse;
    public TimeSpan DrivenTime;
    public string? InfoBoxText;
    public static double TotalCo2Saved { get; private set; }
    public FilterDate SelectedFilterDate;

    public List<FilterDate> FilterDates = new();

    public IndexModel(ILogger<IndexModel> logger, VeloMobilContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult? OnGet(string? errorText)
    {
        if (HttpContext.User.Identities.ToList().First().Name == null) return new RedirectToPageResult(nameof(Login));
        _logger.LogInformation("User {Name} Signed in", HttpContext.User.Identities.ToList().First().Name);

        InfoBoxText = errorText;
        OnCreate();
        Initialize();
        return null;
    }

    private void OnCreate()
    {
        var isCreate = HttpContext.Session.GetString("isCreate");
        if (isCreate != null) return;

        HttpContext.Session.SetString("SelectedFilterDateIndex", "1");
        HttpContext.Session.SetString("startDate", DateTime.Today.ToString(CultureInfo.InvariantCulture));
        HttpContext.Session.SetString("endDate", DateTime.Today.ToString(CultureInfo.InvariantCulture));


        HttpContext.Session.SetString("isCreate", "false");
    }

    private void Initialize()
    {
        TotalCo2Saved = _db.Courses.Sum(x => x.SavedCo2);
        SelectedFilterDateIndex = Convert.ToInt32(HttpContext.Session.GetString("SelectedFilterDateIndex") ?? "0");
        var minCourseDate = _db.Courses.Min(x => x.StartTime);
        FilterDates = new List<FilterDate>
        {
            new()
            {
                StartTime = minCourseDate,
                EndTime = DateTime.Today,
                DayName = "Benutzerdefiniert"
            },
            new()
            {
                StartTime = minCourseDate,
                EndTime = DateTime.Today,
                DayName = "Alle"
            },
            new()
            {
                StartTime = DateTime.Today,
                EndTime = DateTime.Today,
                DayName = "Heute"
            },
            new()
            {
                StartTime = DateTime.Today.AddDays(-1),
                EndTime = DateTime.Today.AddDays(-1),
                DayName = "Gestern"
            },
            new()
            {
                StartTime = DateTime.Today.AddDays(-7),
                EndTime = DateTime.Today,
                DayName = "Letzten 7 Tagen"
            }
        };
        if (SelectedFilterDateIndex == 0)
        {
            SelectedFilterDate = new FilterDate
            {
                StartTime = Convert.ToDateTime(HttpContext.Session.GetString("startDate")),
                EndTime = Convert.ToDateTime(HttpContext.Session.GetString("endDate"))
            };
        }
        else
        {
            SelectedFilterDate = FilterDates[SelectedFilterDateIndex];
        }

        Courses = _db.Courses
            .Include(x => x.DetailPosition)
            .Where(x => x.StartTime.Date >= SelectedFilterDate.StartTime.Date &&
                        x.StartTime.Date <= SelectedFilterDate.EndTime.Date)
            .Select(x => new CourseDto()
            {
                CourseId = x.CourseId,
                Distance = x.Distance,
                Name = x.Name,
                EndTime = x.EndTime,
                MaxSpeed = x.MaxSpeed,
                StartTime = x.StartTime,
                SavedCo2 = x.SavedCo2
            })
            .OrderByDescending(x => x.StartTime)
            .ToList();


        if (!Courses.IsNullOrEmpty())
        {
            try
            {
                SelectedCourseIndex = Convert.ToInt32(HttpContext.Session.GetString("SelectedCourseIndex") ?? "0");
                SelectedCourseId = Convert.ToInt32(HttpContext.Session.GetString("SelectedCourseId") ?? "1");

                SelectedCourse = Courses.SingleOrDefault(x => x.CourseId == SelectedCourseId) ??
                                 new CourseDto();
                DrivenTime = SelectedCourse.EndTime - SelectedCourse.StartTime;
                DetailPositions = _db.DetailPositions
                    .Where(x => x.Courses.CourseId == SelectedCourseId)
                    .Select(x => new DetailPositionDto().CopyFrom(x))
                    .ToList();
            }
            catch (Exception)
            {
                _logger.LogWarning("No route found");
            }
        }
        else
        {
            SelectedCourse = new CourseDto();
            InfoBoxText = "Es wurden keine Fahrten für den ausgewählten Zeitraum gefunden";
        }
    }

    public IActionResult OnPostCourseChanged(string courseId)
    {
        Initialize();
        var item = Courses.SingleOrDefault(x => x.CourseId == int.Parse(courseId));
        int nrInList = Courses.IndexOf(item);
        HttpContext.Session.SetString("SelectedCourseIndex", nrInList.ToString());
        HttpContext.Session.SetString("SelectedCourseId", courseId);
        return new RedirectToPageResult(nameof(Index));
    }

    public IActionResult OnPostTimeFilterChanged(string selectedDay)
    {
        Initialize();
        SelectedFilterDate = FilterDates.Single(x => x.DayName == selectedDay);
        SelectedFilterDateIndex = FilterDates.IndexOf(SelectedFilterDate);

        HttpContext.Session.SetString("SelectedFilterDateIndex", SelectedFilterDateIndex.ToString());

        return new RedirectToPageResult(nameof(Index));
    }

    public IActionResult OnPostSetDate(DateTime startDate, DateTime endDate)
    {
        Initialize();
        if (startDate > endDate) (startDate, endDate) = (endDate, startDate);

        HttpContext.Session.SetString("startDate", startDate.ToString(CultureInfo.InvariantCulture));
        HttpContext.Session.SetString("endDate", endDate.ToString(CultureInfo.InvariantCulture));
        HttpContext.Session.SetString("SelectedFilterDateIndex", "0");
        return new RedirectToPageResult(nameof(Index));
    }

    public async Task<RedirectToPageResult> OnPostLogout()
    {
        _logger.LogInformation("OnPostLogout");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return new RedirectToPageResult(nameof(Login));
    }
}