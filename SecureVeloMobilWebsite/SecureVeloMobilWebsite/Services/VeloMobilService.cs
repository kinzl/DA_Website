namespace SecureVeloMobilWebsite.Services;

public class VeloMobilService
{
    private VeloMobilService _db;
    private ILogger<VeloMobilService> _logger;
    public VeloMobilService(ILogger<VeloMobilService> logger, VeloMobilService db)
    {
        _logger = logger;
        _db = db;
    }
    
    
}