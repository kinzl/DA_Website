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

    public async Task<ActionResult> AddPositionsToNewCourse(Course course)
    {
        if (!course.DetailPosition.IsNullOrEmpty())
        {
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
        }
        else
        {
            _db.Courses.Add(new Course()
            {
                DetailPosition = new List<DetailPosition>(),
                Name = course.Name,
                Distance = course.Distance,
                MaxSpeed = course.MaxSpeed,
                EndTime = course.EndTime,
                StartTime = course.StartTime,
                SavedCo2 = course.SavedCo2,
            });
        }

        await _db.SaveChangesAsync();
        var lastCourseId = _db.Courses.OrderBy(x => x.CourseId).Last().CourseId;
        Console.WriteLine(lastCourseId);
        return Ok(lastCourseId);
    }

    public async Task<ActionResult> AddPositionsToExistingCourse(Course course)
    {
        //ToDO: calculate full distance and not only the new one
        if (course.DetailPosition.IsNullOrEmpty()) return BadRequest("Detail Positions are empty");

        foreach (var position in course.DetailPosition)
        {
            position.PosZ = await GetAltitudeAsync(position.PosY, position.PosX);
        }

        var selectedCourse = _db.Courses
            .Include(x => x.DetailPosition)
            .SingleOrDefault(x => x.CourseId == course.CourseId)!;

        selectedCourse.DetailPosition.AddRange(course.DetailPosition);
        selectedCourse.Distance += CalculateDistance(course);
        selectedCourse.SavedCo2 += CalculateSavedCo2(selectedCourse.Distance);

        await _db.SaveChangesAsync();

        return Ok("Added positions to " + course.CourseId);
    }

    private double CalculateDistance(Course course)
    {
        double distance = 0;

        for (int i = 0; i < course.DetailPosition.Count - 1; i++)
        {
            var d1 = course.DetailPosition[i].PosY * (Math.PI / 180.0);
            var num1 = course.DetailPosition[i].PosX * (Math.PI / 180.0);
            var d2 = course.DetailPosition[i + 1].PosY * (Math.PI / 180.0);
            var num2 = course.DetailPosition[i + 1].PosX * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            distance += 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        return distance / 1000;
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