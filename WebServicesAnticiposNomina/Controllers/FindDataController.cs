using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FindDataController : ControllerBase
    {
        public IConfiguration _configuration;

        public FindDataController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // POST api/<AdvanceController>
        [HttpPost]
        public ResponseModels Post([FromHeader] string Token, [FromBody] GetContractRequest getContractRequest)
        {
            ResponseModels responseModels = new();
            try
            {
                AdvanceCore advanceCore = new(_configuration);
                responseModels = advanceCore.GetContract(getContractRequest, Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}