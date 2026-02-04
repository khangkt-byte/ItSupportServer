using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ItSupportServer.src.Modules.Area
{
    [Route("api/areas")]
    [ApiController]
    public class AreasController(IAreasService services) : ControllerBase
    {
    }
}
