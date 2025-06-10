using Microsoft.AspNetCore.Mvc;
using System.Data;
using WebServicesAnticiposNomina.Core;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutomaticValidationAdvancesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AutomaticValidationAdvancesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        public ActionResult Get()
        {
            AdvanceCore advanceCore = new AdvanceCore(_configuration);
            int code = advanceCore.AutomaticValidationAdvances();
            switch (code)
            {
                case 200:
                    return Ok("Correo enviado");
                case 500:
                    return BadRequest("Error interno");
                case 204:
                    return NoContent();
                default:
                    return BadRequest("Error interno");
            }
        }
    }
}
