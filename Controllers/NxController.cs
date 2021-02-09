using Microsoft.AspNetCore.Mvc;

namespace IntegrationNxWitness.AddControllersWithViews
{
    [ApiController]
    [Route("api/v1/[controller]/[action]")]
    public class NxController : Controller
    {
        [HttpGet]
        public IActionResult Test()
        {
            return Ok("Test");
        }
    }    
}