using Microsoft.AspNetCore.Mvc;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangePasswordController : ControllerBase
    {
        public IConfiguration _configuration;

        public ChangePasswordController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ResponseModels Get(string ID)
        {
            ResponseModels responseModels = new();
            try
            {
                UserCore userCore = new(_configuration);
                responseModels = userCore.SendCodeRecovery(ID);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }

        [HttpPut]
        public ResponseModels ResponseModels([FromHeader] string Token, [FromBody] UpdatePasswordRequest updatePasswordRequest)
        {
            ResponseModels responseModels = new();
            try
            {
                UserCore userCore = new(_configuration);
                responseModels = userCore.PutPassword(updatePasswordRequest, Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}