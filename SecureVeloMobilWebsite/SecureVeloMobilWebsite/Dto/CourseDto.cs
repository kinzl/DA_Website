namespace SecureVeloMobilWebsite.Dto;

public class CourseDto
{
    public int CourseId { get; set; }
    public List<DetailPositionDto>? DetailPosition { get; set; }
    public string? Name { get; set; }
    public double Distance { get; set; }
    public double SavedCo2 { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}