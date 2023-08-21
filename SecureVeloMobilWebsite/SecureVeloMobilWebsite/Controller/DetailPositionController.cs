using Microsoft.AspNetCore.Mvc;
using SecureVeloMobilWebsite.Services;

namespace SecureVeloMobilWebsite.Controller;

public class DetailPositionController
{
    private VeloMobilService _service;

    public DetailPositionController(VeloMobilService service)
    {
        _service = service;
    }

    public ActionResult GetFirstPosition()
    {
        return null;
    }
}