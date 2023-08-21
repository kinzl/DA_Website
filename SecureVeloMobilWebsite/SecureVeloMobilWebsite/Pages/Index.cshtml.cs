using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using SecureVeloMobilWebsite.Dto;
using SecureVeloMobilWebsite.wwwroot.Extensions;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private VeloMobilContext _db;
    private Seeder _seeder;
    public List<CourseDto> Courses;
    public int SelectedCourseIndex;
    public int SelectedCourseId;
    public List<DetailPositionDto> DetailPositions;
    public DetailPositionDto LastDetailPosition { get; set; }

    public IndexModel(ILogger<IndexModel> logger, VeloMobilContext db, Seeder seeder)
    {
        _logger = logger;
        _db = db;
        _seeder = seeder;
    }

    public void OnGet()
    {
        // _db.Database.EnsureDeleted();
        // _db.Database.EnsureCreated();
        // _seeder.Seed();
        Initialize();
    }

    private void Initialize()
    {
        Courses = _db.Courses
            .Select(x => new CourseDto().CopyFrom(x))
            .ToList();
        if (!Courses.IsNullOrEmpty())
        {
            SelectedCourseIndex = Convert.ToInt32(HttpContext.Session.GetString("SelectedCourseIndex") ?? "0");
            SelectedCourseId = Convert.ToInt32(HttpContext.Session.GetString("SelectedCourseId") ?? "1");
            DetailPositions = _db.DetailPositions
                .Where(x => x.Courses.CourseId == SelectedCourseId)
                .Select(x => new DetailPositionDto()
                {
                    DateTime = x.DateTime,
                    PosY = x.PosY,
                    PosX = x.PosX,
                    DetailPositionId = x.DetailPositionId,
                })
                .ToList();
            LastDetailPosition = DetailPositions.OrderBy(x => x.DateTime)
                .Last();
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
}