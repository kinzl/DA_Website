using System.Runtime.InteropServices.JavaScript;
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
    public string InfoBoxText = "";
    public static int TotalCo2Saved { get; private set; }
    public FilterDate SelectedFilterDate;

    public List<FilterDate> FilterDates = new()
    {
        new()
        {
            startTime = DateTime.MinValue,
            endTime = DateTime.Today,
            DayName = "Benutzerdefiniert",
        },
        new()
        {
            startTime = DateTime.MinValue,
            endTime = DateTime.Today,
            DayName = "Alle",
        },
        new()
        {
            startTime = DateTime.Today,
            endTime = DateTime.Today,
            DayName = "Heute",
        },
        new()
        {
            startTime = DateTime.Today.AddDays(-1),
            endTime = DateTime.Today.AddDays(-1),
            DayName = "Gestern",
        },
        new()
        {
            startTime = DateTime.Today.AddDays(-7),
            endTime = DateTime.Today,
            DayName = "Letzten 7 Tagen",
        },
    };

    public DetailPositionDto LastDetailPosition;

    public IndexModel(ILogger<IndexModel> logger, VeloMobilContext db)
    {
        _logger = logger;
        _db = db;

    }

    public void OnGet()
    {
        OnCreate();
        Initialize();
    }

    private void OnCreate()
    {
        var isCreate = HttpContext.Session.GetString("isCreate");
        if (isCreate != null) return;
        HttpContext.Session.SetString("SelectedFilterDateIndex", "1");
        HttpContext.Session.SetString("startDate", DateTime.Today.ToString());
        HttpContext.Session.SetString("endDate", DateTime.Today.ToString());

        HttpContext.Session.SetString("isCreate", "false");
    }

    private void Initialize()
    {
        TotalCo2Saved = _db.Courses.Select(x => x.SavedCo2).Count();
        SelectedFilterDateIndex = Convert.ToInt32(HttpContext.Session.GetString("SelectedFilterDateIndex") ?? "0");
        if (SelectedFilterDateIndex == 0)
        {
            SelectedFilterDate = new FilterDate();
            SelectedFilterDate.startTime = Convert.ToDateTime(HttpContext.Session.GetString("startDate"));
            SelectedFilterDate.endTime = Convert.ToDateTime(HttpContext.Session.GetString("endDate"));
        }
        else
        {
            SelectedFilterDate = FilterDates[SelectedFilterDateIndex];
        }

        Courses = _db.Courses
            .Include(x => x.DetailPosition)
            .Where(x => x.StartTime.Date >= SelectedFilterDate.startTime.Date &&
                        x.StartTime.Date <= SelectedFilterDate.endTime.Date)
            .Select(x => new CourseDto()
            {
                CourseId = x.CourseId,
                Distance = x.Distance,
                Name = x.Name,
                EndTime = x.EndTime,
                MaxSpeed = x.MaxSpeed,
                StartTime = x.StartTime,
                SavedCo2 = x.SavedCo2,
            })
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
                    .Select(x => new DetailPositionDto()
                    {
                        PositionTime = x.PositionTime,
                        PosY = x.PosY,
                        PosX = x.PosX,
                        PosZ = x.PosZ,
                        DetailPositionId = x.DetailPositionId,
                    })
                    .ToList();
                LastDetailPosition = DetailPositions.OrderBy(x => x.PositionTime)
                    .Last();
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
        var item = Courses.Where(x => x.CourseId == int.Parse(courseId)).Single();
        int nrInList = Courses.IndexOf(item);
        HttpContext.Session.SetString("SelectedCourseIndex", nrInList.ToString());
        HttpContext.Session.SetString("SelectedCourseId", courseId.ToString());
        return new RedirectToPageResult("Index");
    }

    public IActionResult OnPostTimeFilterChanged(string selectedDay)
    {
        Initialize();
        SelectedFilterDate = FilterDates.Where(x => x.DayName == selectedDay).Single();
        SelectedFilterDateIndex = FilterDates.IndexOf(SelectedFilterDate);

        HttpContext.Session.SetString("SelectedFilterDateIndex", SelectedFilterDateIndex.ToString());

        return new RedirectToPageResult("Index");
    }

    public IActionResult OnPostSetDate(DateTime startDate, DateTime endDate)
    {
        Initialize();
        if (startDate > endDate) (startDate, endDate) = (endDate, startDate);

        HttpContext.Session.SetString("startDate", startDate.ToString());
        HttpContext.Session.SetString("endDate", endDate.ToString());
        HttpContext.Session.SetString("SelectedFilterDateIndex", "0");
        return new RedirectToPageResult("Index");
    }
}