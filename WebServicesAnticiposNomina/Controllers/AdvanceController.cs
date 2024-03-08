using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvanceController : ControllerBase
    {
        public IConfiguration _configuration;

        public AdvanceController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // POST api/<AdvanceController>
        [HttpPost]
        public ResponseModels Post([FromHeader]string Token, [FromBody]AdvanceRequest AdvanceRequest)
        {
            ResponseModels responseModels = new();
            try
            {
                AdvanceCore advanceCore = new(_configuration);
                responseModels = advanceCore.PostCodeAdvance(AdvanceRequest, Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
         
        // PUT api/<AdvanceController>/5
        [HttpPut]
        public ResponseModels Put([FromHeader] string Token, [FromBody] AdvanceRequest advanceRequest)
        {
            ResponseModels responseModels = new();
            try
            {
                AdvanceCore advanceCore = new(_configuration);
                responseModels = advanceCore.PostAdvance(advanceRequest, Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}
