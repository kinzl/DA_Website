using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SecureVeloMobilWebsite.Controller;
using SecureVeloMobilWebsite.Dto;
using SecureVeloMobilWebsite.wwwroot.Extensions;
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

    public ActionResult AddPositionsToNewCourse(Course course)
    {
        course.Distance = CalculateDistance(course);
        course.SavedCo2 = CalculateSavedCo2(course.Distance);
        _db.Courses.Add(new Course()
        {
            DetailPosition = course.DetailPosition,
            Name = course.Name,
            Distance = course.Distance,
            MaxSpeed = course.MaxSpeed,
            EndTime = course.EndTime,
            StartTime = course.StartTime,
            DayOfRecording = course.DayOfRecording,
            SavedCo2 = course.SavedCo2
        });
        _db.SaveChanges();
        var lastCourseId = _db.Courses.OrderBy(x => x.CourseId).Last().CourseId;
        Console.WriteLine(lastCourseId);
        return Ok(lastCourseId);
    }

    public ActionResult AddPositionsToExistingCourse(Course course)
    {
        course.Distance = CalculateDistance(course);
        course.SavedCo2 = CalculateSavedCo2(course.Distance);
        var selectedCourse = _db.Courses
            .Include(x => x.DetailPosition)
            .SingleOrDefault(x => x.CourseId == course.CourseId)!
            .DetailPosition;


        foreach (var item in course.DetailPosition)
        {
            selectedCourse.Add(item);
        }

        _db.SaveChanges();

        return Ok("Added positions to " + course.CourseId);
    }

    private double CalculateDistance(Course course)
    {
        double distance = 0;

        for (int i = 0; i < course.DetailPosition.Count - 1; i++)
        {
            var point1 = course.DetailPosition[i];
            var point2 = course.DetailPosition[i + 1];

            double deltaX = point2.PosX - point1.PosX;
            double deltaY = point2.PosY - point1.PosY;

            // Calculate Euclidean distance between two points
            double segmentDistance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Add the distance to the total distance
            distance += segmentDistance;
        }

        return distance;
    }

    private double CalculateSavedCo2(double distance)
    {
        return (distance * MyConstants.co2FootprintCarInGram - distance * MyConstants.co2FootprintBikeInGram) / 1000;
    }
}