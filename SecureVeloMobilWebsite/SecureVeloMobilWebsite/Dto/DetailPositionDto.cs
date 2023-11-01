using System.Text.Json.Serialization;

namespace SecureVeloMobilWebsite.Dto;

public class DetailPositionDto
{
    public int DetailPositionId { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public DateTime PositionTime { get; set; }
    public int CoursesId { get; set; }
}