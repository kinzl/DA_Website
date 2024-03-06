using Microsoft.AspNetCore.Mvc;
using SecureVeloMobilWebsite.Dto;
using SecureVeloMobilWebsite.Services;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Controller;

[ApiController]
[Route("[controller]/[action]")]
public class DetailPositionController : ControllerBase
{
    private VeloMobilService _service;

    public DetailPositionController(VeloMobilService service)
    {
        _service = service;
    }

    [HttpPut]
    public ActionResult AddPositionsToExistingCourse([FromBody] CourseDto course)
    {
        return _service.AddPositionsToExistingCourse(new Course()
        {
            Name = course.Name ?? "",
            DetailPosition = course.DetailPosition.Select(x => new DetailPosition().CopyFrom(x)).ToList(),
            StartTime = course.StartTime,
            MaxSpeed = course.MaxSpeed,
            CourseId = course.CourseId,
            EndTime = course.EndTime
        });
    }

    [HttpPost]
    public ActionResult AddPositionsToNewCourse([FromBody] CourseDto positions)
    {
        var newCourse = new Course()
        {
            Name = positions.Name ?? "",
            DetailPosition = positions.DetailPosition.Select(x => new DetailPosition().CopyFrom(x)).ToList(),
            StartTime = positions.StartTime,
            MaxSpeed = positions.MaxSpeed,
            EndTime = positions.EndTime
        };

        return _service.CreateNewCourse(newCourse);
    }

    [HttpPost("{courseId}")]
    public ActionResult CalculateAltitudeFromCourse(int courseId)
    {
        return _service.AddAltitudeToCourse(courseId);
    }
}