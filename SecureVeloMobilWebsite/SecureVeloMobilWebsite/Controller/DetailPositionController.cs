using Microsoft.AspNetCore.Mvc;
using SecureVeloMobilWebsite.Dto;
using SecureVeloMobilWebsite.Services;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Controller;

[ApiController]
[Route("[controller]/[action]")]
public class DetailPositionController
{
    private VeloMobilService _service;

    public DetailPositionController(VeloMobilService service)
    {
        _service = service;
    }

    [HttpPost]
    public ActionResult AddPositionsToNewCourse([FromBody] CourseDto positions)
    {
        var newCourse = new Course()
        {
            Name = positions.Name ?? "",
            DetailPosition = positions.DetailPosition.Select(x => new DetailPosition().CopyFrom(x)).ToList(),
            StartTime = positions.StartTime,
            EndTime = positions.EndTime
        };

        return _service.CreateNewCourse(newCourse);
    }

    [HttpPut]
    public async Task<ActionResult> AddPositionsToExistingCourse([FromBody] CourseDto course)
    {
        return await _service.AddPositionsToExistingCourse(new Course()
        {
            Name = course.Name ?? "",
            DetailPosition = course.DetailPosition.Select(x => new DetailPosition().CopyFrom(x)).ToList(),
            StartTime = course.StartTime,
            CourseId = course.CourseId,
            EndTime = course.EndTime
        });
    }
    // [HttpPut("{courseId}")]
    // public async Task<ActionResult> CalculateAltitudeFromCourse(int courseId)
    // {
    //     return await _service.AddAltitudeToCourse(courseId);
    // }
}