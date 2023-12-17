using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeloMobilDb;

public partial class Course
{
    public int CourseId { get; set; }
    public List<DetailPosition> DetailPosition { get; set; }
    public string Name { get; set; }
    public double Distance { get; set; }
    public double MaxSpeed { get; set; }
    public double SavedCo2 { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime DayOfRecording { get; set; }
    
}
