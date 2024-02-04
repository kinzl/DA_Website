using System.Globalization;
using SecureVeloMobilWebsite.wwwroot.Extensions;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.Services;

public class StartupBackgroundService : BackgroundService
{
    private readonly IServiceScope _scope;
    public StartupBackgroundService(IServiceProvider provider)
    {
        _scope = provider.CreateScope();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _scope.ServiceProvider.GetRequiredService<VeloMobilContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        
        Seed(db);
        
        return Task.Run(() => db.SaveChanges(), stoppingToken);
    }

   private void Seed(VeloMobilContext db)
    {
        var course1 = new Course()
        {
            CourseId = 0,
            Name = "Peuerbach - Grieskirchen",
            StartTime = DateTime.Today.AddDays(-1).AddHours(11),
            EndTime = DateTime.Today,
            DetailPosition = new List<DetailPosition>()
            {
                // Peuerbach
                new DetailPosition()
                {
                    PosX = 13.7672137f,
                    PosY = 48.3672354f,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:33", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                },
                // Grieskirchen
                new DetailPosition()
                {
                    PosX = 13.8187544f,
                    PosY = 48.2148842f,
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
                new DetailPosition()
                {
                    PosX = 14.28611f,
                    PosY = 48.30639f,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:55", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                },
                // Graz
                new DetailPosition()
                {
                    PosX = 15.500000f,
                    PosY = 47.300000f,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:56", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                },
                // Linz
                new DetailPosition()
                {
                    PosX = 16.363449f,
                    PosY = 48.210033f,
                    PositionTime = DateTime.ParseExact("31.07.2023 10:57", "dd.MM.yyyy hh:mm",
                        CultureInfo.InvariantCulture),
                }
            }
        };
        db.Courses.Add(course2);
        // var course3 = new Course()
        // {
        //     CourseId = 0,
        //     Name = "Linz - Graz - Wien",
        //     StartTime = DateTime.Today.AddDays(-8).AddHours(2),
        //     EndTime = DateTime.Today.AddDays(-7),
        //     DetailPosition = new List<DetailPosition>()
        //     {
        //         // Wien
        //         new DetailPosition()
        //         {
        //             PosX = 14.28611f,
        //             PosY = 48.30639f,
        //             PositionTime = DateTime.ParseExact("31.07.2023 10:55", "dd.MM.yyyy hh:mm",
        //                 CultureInfo.InvariantCulture),
        //         },
        //         // Graz
        //         new DetailPosition()
        //         {
        //             PosX = 15.500000f,
        //             PosY = 47.300000f,
        //             PositionTime = DateTime.ParseExact("31.07.2023 10:56", "dd.MM.yyyy hh:mm",
        //                 CultureInfo.InvariantCulture),
        //         },
        //         // Linz
        //         new DetailPosition()
        //         {
        //             PosX = 16.363449f,
        //             PosY = 48.210033f,
        //             PositionTime = DateTime.ParseExact("31.07.2023 10:57", "dd.MM.yyyy hh:mm",
        //                 CultureInfo.InvariantCulture),
        //         }
        //     }
        // };
        // db.Courses.Add(course3);
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

        return distance/1000;
    }

    private double CalculateSavedCo2(double distance)
    {
        return (distance * MyConstants.co2FootprintCarInGram - distance * MyConstants.co2FootprintBikeInGram) / 1000;
    }
}