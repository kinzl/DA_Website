using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureVeloMobilWebsite.Controller;
using SecureVeloMobilWebsite.Dto;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Services;

public class VeloMobilService
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
        int maxSpeed = 0;
        detailPositions.DetailPosition.ForEach(x =>
        {
            
        });
        _db.Courses.Add(new Course()
        {
            DetailPosition = detailPositions.DetailPosition,
            Name = detailPositions.Name,
            MaxSpeed = maxSpeed,
        });
        _db.SaveChanges();
        return new OkResult();
    }
}