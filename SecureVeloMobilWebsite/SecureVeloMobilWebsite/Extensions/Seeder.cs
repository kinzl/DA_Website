using System.Globalization;
using VeloMobilDb;

namespace SecureVeloMobilWebsite.wwwroot.Extensions;

public class Seeder
{
    private VeloMobilContext _db;

    public Seeder(VeloMobilContext db)
    {
        _db = db;
    }

    public void Seed()
    {
        var course1 = new Course()
        {
            CourseId = 0,
            Name = "Peuerbach - Grieskirchen - Peuerbach",
            Visible = true,
            DetailPosition = new List<DetailPosition>()
            {
                // Peuerbach
                new DetailPosition()
                {
                    PosX = 13.7672137f,
                    PosY = 48.3672354f,
                    DateTime = DateTime.ParseExact("31.07.2023 10:33","dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture),
                },
                // Grieskirchen
                new DetailPosition()
                {
                    PosX = 13.8187544f,
                    PosY = 48.2148842f,
                    DateTime = DateTime.ParseExact("31.07.2023 10:41","dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture),
                }
            }
        };
        _db.Courses.Add(course1);
        
        var course2 = new Course()
        {
            CourseId = 0,
            Name = "Linz - Graz - Wien",
            Visible = true,
            DetailPosition = new List<DetailPosition>()
            {
                // Wien
                new DetailPosition()
                {
                    PosX = 14.28611f,
                    PosY = 48.30639f,
                    DateTime = DateTime.ParseExact("31.07.2023 10:55","dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture),
                },
                // Graz
                new DetailPosition()
                {
                    PosX = 15.500000f,
                    PosY = 47.300000f,
                    DateTime = DateTime.ParseExact("31.07.2023 10:56","dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture),
                },
                // Linz
                new DetailPosition()
                {
                    PosX = 16.363449f,
                    PosY = 48.210033f,
                    DateTime = DateTime.ParseExact("31.07.2023 10:57","dd.MM.yyyy hh:mm", CultureInfo.InvariantCulture),
                }
            }
        };
        _db.Courses.Add(course2);
        _db.SaveChanges();
    }
}