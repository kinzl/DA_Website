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

    [HttpGet("{courseId}")]
    public List<DetailPositionDto> GetPositionsByCourseId(int courseId)
    {
        return _service.GetPositionsByCourseId(courseId)
            .Select(x => new DetailPositionDto().CopyFrom(x))
            .ToList();
    }

    [HttpPost]
    public ActionResult AddPositionsToNewCourse([FromBody] PostCourseDto positions)
    {
        var p = new Course()
        {
            Name = positions.Name,
            CourseId = positions.CourseId,
            DetailPosition = positions.DetailPosition.Select(x => new DetailPosition().CopyFrom(x)).ToList(),
            Distance = positions.Distance,
            Picture = positions.Picture,
            Visible = positions.Visible,
            DrivenTime = positions.DrivenTime,
            MaxSpeed = positions.MaxSpeed,
        };
        _service.AddPositionsToNewCourse(p);
        return new OkResult();
    }
}