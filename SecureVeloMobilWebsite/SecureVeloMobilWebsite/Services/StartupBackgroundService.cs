using System.Globalization;
using SecureVeloMobilWebsite.Extensions;
using SecureVeloMobilWebsite.wwwroot.Extensions;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Services;

public class StartupBackgroundService : BackgroundService
{
    private readonly IServiceScope _scope;
    private PasswordEncryption _pe;

    public StartupBackgroundService(IServiceProvider provider)
    {
        _scope = provider.CreateScope();
        _pe = provider.GetRequiredService<PasswordEncryption>();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("ExecuteAsync STARTUPSERVICE");
        var db = _scope.ServiceProvider.GetRequiredService<VeloMobilContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        Seed(db);

        return Task.Run(() => db.SaveChanges(), stoppingToken);
    }

    private void Seed(VeloMobilContext db)
    {
        Console.WriteLine("SEED STARTUP SERVICE");
        var hashedPw = _pe.HashPassword("foobar");
        var user = new User()
        {
            Username = "TWelsch",
            PasswordHash = hashedPw
        };
        db.Users.Add(user);

        var course1 = new Course()
        {
            CourseId = 0,
            Name = "Peuerbach - Grieskirchen",
            StartTime = DateTime.Today.AddDays(-1).AddHours(11),
            EndTime = DateTime.Today,
            DetailPosition = new List<DetailPosition>()
            {
                // Peuerbach
                new()
                {
                    PosX = 13.7672137f,
                    PosY = 48.3672354f,
                    PosZ = 420,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:33", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                },
                // Grieskirchen
                new()
                {
                    PosX = 13.8187544f,
                    PosY = 48.2148842f,
                    PosZ = 425,
                    CurrentSpeed = 32,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:41", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                }
            }
        };
        course1.Distance = CalculateDistance(course1);
        course1.SavedCo2 = CalculateSavedCo2(course1.Distance);
        db.Courses.Add(course1);
        
        var course2 = new Course()
        {
            CourseId = 0,
            Name = "Linz - Graz - Wien",
            StartTime = DateTime.Today.AddDays(-1).AddHours(-2),
            EndTime = DateTime.Today,
            DetailPosition = new List<DetailPosition>()
            {
                // Wien
                new()
                {
                    PosX = 14.28611f,
                    PosY = 48.30639f,
                    PosZ = 350,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:55", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                },
                // Graz
                new()
                {
                    PosX = 15.500000f,
                    PosY = 47.300000f,
                    PosZ = 300,
                    CurrentSpeed = 4003,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:56", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                },
                // Linz
                new()
                {
                    PosX = 16.363449f,
                    PosY = 48.210033f,
                    PosZ = 400,
                    CurrentSpeed = 67,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:57", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                }
            }
        };
        db.Courses.Add(course2);
        db.SaveChanges();
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
        return (distance * MyConstants.Co2FootprintCarInGram - distance * MyConstants.Co2FootprintBikeInGram) / 1000;
    }
}