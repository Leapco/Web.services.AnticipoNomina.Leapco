using Microsoft.AspNetCore.Mvc;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproveAdvanceController : ControllerBase
    {
        public IConfiguration _configuration;

        public ApproveAdvanceController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPut]
        public ResponseModels Put([FromHeader] string Token, [FromBody] AdvanceRequest AdvanceRequest)
        {
            ResponseModels responseModels = new();
            try
            {
                AdvanceCore advanceCore = new(_configuration);
                responseModels = advanceCore.PutStatudAdvance(AdvanceRequest, Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}