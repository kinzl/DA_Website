using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

    public ActionResult NewCourse(Course course)
    {
        _db.Courses.Add(new Course()
        {
            DetailPosition = new List<DetailPosition>(),
            Name = course.Name,
            Distance = 0,
            MaxSpeed = 0,
            EndTime = course.EndTime,
            StartTime = course.StartTime,
            SavedCo2 = 0,
        });

        _db.SaveChanges();
        var lastCourseId = _db.Courses.OrderBy(x => x.CourseId).Last().CourseId;
        _logger.LogInformation("new course created with id: ");
        _logger.LogInformation(lastCourseId.ToString());
        return Ok(lastCourseId);
    }

    public ActionResult ExistingCourse(Course course)
    {
        _logger.LogInformation("Existing course");
        if (course.DetailPosition.IsNullOrEmpty() || course.CourseId == 0)
            return BadRequest("Detail Positions or courseId empty");
        foreach (var position in course.DetailPosition)
        {
            position.PosZ = 0;
        }

        var selectedCourse = _db.Courses
            .Include(x => x.DetailPosition)
            .SingleOrDefault(x => x.CourseId == course.CourseId);

        selectedCourse.DetailPosition.AddRange(course.DetailPosition);
        selectedCourse.Distance = CalculateDistance(selectedCourse);
        selectedCourse.SavedCo2 = CalculateSavedCo2(selectedCourse.Distance);
        selectedCourse.EndTime = course.EndTime;
        selectedCourse.MaxSpeed = selectedCourse.DetailPosition.Max(x => x.CurrentSpeed);

        _db.SaveChanges();
        return Ok("Added positions to " + course.CourseId);
    }

    public ActionResult AddAltitudeToCourse(int courseId)
    {
        var selectedCourse = _db.Courses
            .Include(x => x.DetailPosition)
            .Where(x => x.CourseId == courseId)
            .Select(x => x.DetailPosition)
            .SingleOrDefault();
        if (selectedCourse == null) return BadRequest("Course does not exist");
        foreach (var position in selectedCourse)
        {
            position.PosZ = CalculateAltitude(position.PosY, position.PosX);
            Thread.Sleep(100);
        }

        _db.SaveChanges();
        return Ok();
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

            double currentDistance = 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
            distance += currentDistance;
            CalculateMaxSpeed(currentDistance, course.DetailPosition[i], course.DetailPosition[i + 1]);
        }

        return distance / 1000;
    }

    private void CalculateMaxSpeed(double distance, DetailPosition firstPosition, DetailPosition secondPosition)
    {
        double timeDifference = (secondPosition.PositionTime - firstPosition.PositionTime).TotalSeconds;
        secondPosition.CurrentSpeed = distance / timeDifference * 3.6;
    }

    private double CalculateSavedCo2(double distance)
    {
        return (distance * MyConstants.co2FootprintCarInGram - distance * MyConstants.co2FootprintBikeInGram) / 1000;
    }

    double CalculateAltitude(double latitude, double longitude)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            string apiUrl = $"https://api.open-elevation.com/api/v1/lookup?locations={latitude},{longitude}";

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(apiUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
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