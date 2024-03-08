using Microsoft.AspNetCore.Mvc;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivateUserController : ControllerBase
    {
        public IConfiguration _configuration;

        public ActivateUserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost]
        public ResponseModels Post([FromBody] ActivateUserResponse activateUserResponse)
        {
            ResponseModels responseModels = new();
            try
            {
                UserCore userCore = new(_configuration);
                responseModels = userCore.SendCodeActivate(activateUserResponse);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }

        [HttpPut]
        public ResponseModels Put([FromHeader] string Token, [FromBody] UpdatePasswordRequest updatePasswordRequest)
        {
            ResponseModels responseModels = new();
            try
            {
                UserCore userCore = new(_configuration);
                responseModels = userCore.PutPasswordActivate(updatePasswordRequest, Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}
