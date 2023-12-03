using System.Text.Json.Serialization;

namespace SecureVeloMobilWebsite.Dto;

public class PostCourseDto
{
    public List<DetailPositionDto> DetailPosition { get; set; }
    public string Name { get; set; }
    public float Picture { get; set; }
    public bool Visible { get; set; }
    public double Distance { get; set; }
    public double MaxSpeed { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime DayOfRecording { get; set; }
}