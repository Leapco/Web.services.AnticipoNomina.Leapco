using Microsoft.AspNetCore.Mvc;
using WebServicesAnticiposNomina.Core;

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FixContractController : Controller
    {
        public IConfiguration _configuration;

        public FixContractController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        // POST
        [HttpPost]
        public string Post(List<int> id_Anticipos)
        {
            try
            {
                AdvanceCore advanceCore = new(_configuration);
                string response = advanceCore.crearContratos(id_Anticipos);
                return response;
            }
            catch
            {
                return null;
            }
        }
    }
}
