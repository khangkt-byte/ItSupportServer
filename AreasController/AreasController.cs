using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITSupportServer.AreasController
{
    [Route("api/areas")]
    [ApiController]
    public class AreasController(IAreasServices services) : ControllerBase
    {
    }
}
