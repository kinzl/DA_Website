namespace VeloMobilDb;

public partial class DetailPosition
{
    public int DetailPositionId { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public double PosZ { get; set; }
    public DateTime PositionTime { get; set; }
    public Course Courses { get; set; }
}