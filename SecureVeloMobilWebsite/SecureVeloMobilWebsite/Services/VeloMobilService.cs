using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureVeloMobilWebsite.Controller;
using SecureVeloMobilWebsite.Dto;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Services;

public class VeloMobilService : ControllerBase
{
    private VeloMobilContext _db;
    private ILogger<VeloMobilService> _logger;

    public VeloMobilService(ILogger<VeloMobilService> logger, VeloMobilContext db)
    {
        _logger = logger;
        _db = db;
    }

    public List<DetailPosition> GetPositionsByCourseId(int courseId)
    {
        return _db.DetailPositions
            .Include(x => x.Courses)
            .Where(x => x.Courses.CourseId == courseId)
            .ToList();
    }

    public ActionResult AddPositionsToNewCourse(Course detailPositions)
    {
        _db.Courses.Add(new Course()
        {
            DetailPosition = detailPositions.DetailPosition,
            Name = detailPositions.Name,
            MaxSpeed = 0,
        });
        _db.SaveChanges();
        var lastCourseId = _db.Courses.OrderBy(x => x.CourseId).Last().CourseId;
        return Ok(lastCourseId);
    }

    public ActionResult AddPositionsToExistingCourse(Course course)
    {
        var selectedCourse = _db.Courses
            .Include(x => x.DetailPosition)
            .SingleOrDefault(x => x.CourseId == course.CourseId)
            .DetailPosition;

        foreach (var item in course.DetailPosition)
        {
            selectedCourse.Add(item);
        }

        _db.SaveChanges();

        return Ok("Added positions to " + course.CourseId);
    }
}