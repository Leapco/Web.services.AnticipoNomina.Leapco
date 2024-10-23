using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeebHookController : ControllerBase
    {
        public IConfiguration _configuration;

        public WeebHookController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public string Post([FromBody] TransactionRequest transactionRequest)
        {
            string? responseModels;
            try
            {
                ApiCobreCore apiCobreCore = new(_configuration);
                responseModels = apiCobreCore.WeebHookPayment(transactionRequest);
            }
            catch (Exception)
            {
                responseModels = "500";
            }
            return responseModels;
        }
    }
}