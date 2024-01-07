using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

    public async Task<ActionResult> AddPositionsToNewCourse(Course course)
    {
        if (course.DetailPosition.IsNullOrEmpty()) return Ok("No Positions found");
        course.Distance = CalculateDistance(course);
        course.SavedCo2 = CalculateSavedCo2(course.Distance);

        foreach (var position in course.DetailPosition)
        {
            position.PosZ = await GetAltitudeAsync(position.PosY, position.PosX);
        }

        _db.Courses.Add(new Course()
        {
            DetailPosition = course.DetailPosition,
            Name = course.Name,
            Distance = course.Distance,
            MaxSpeed = course.MaxSpeed,
            EndTime = course.EndTime,
            StartTime = course.StartTime,
            SavedCo2 = course.SavedCo2,
        });
        await _db.SaveChangesAsync();
        var lastCourseId = _db.Courses.OrderBy(x => x.CourseId).Last().CourseId;
        Console.WriteLine(lastCourseId);
        return Ok(lastCourseId);
    }

    public async Task<ActionResult> AddPositionsToExistingCourse(Course course)
    {
        if (course.DetailPosition.IsNullOrEmpty()) return BadRequest("No Positions found");
        course.Distance = CalculateDistance(course);
        course.SavedCo2 = CalculateSavedCo2(course.Distance);

        foreach (var position in course.DetailPosition)
        {
            position.PosZ = await GetAltitudeAsync(position.PosY, position.PosX);
        }

        var selectedCourse = _db.Courses
            .Include(x => x.DetailPosition)
            .SingleOrDefault(x => x.CourseId == course.CourseId)!
            .DetailPosition;

        selectedCourse.AddRange(course.DetailPosition);

        await _db.SaveChangesAsync();

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

        return (distance / 1000);
    }

    private double CalculateSavedCo2(double distance)
    {
        return (distance * MyConstants.co2FootprintCarInGram - distance * MyConstants.co2FootprintBikeInGram) / 1000;
    }

    async Task<double> GetAltitudeAsync(double latitude, double longitude)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            string apiUrl = $"https://api.open-elevation.com/api/v1/lookup?locations={latitude},{longitude}";

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    // Extract altitude from the API response
                    double altitude = result.results[0].elevation;
                    return altitude;
                }

                Console.WriteLine($"API request failed: {response.StatusCode}");
                return double.NaN;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return double.NaN;
            }
        }
    }
}