using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Response;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidateActiveUserController : ControllerBase
    {
        public IConfiguration _configuration;

        public ValidateActiveUserController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // GET: api/<ValidateActiveUserController>
        [HttpGet]
        public ResponseModels Get([FromHeader] string Token, string ID, string Code)
        {
            ResponseModels responseModels = new();
            try
            {
                UserCore userCore = new(_configuration);
                responseModels = userCore.ValidateActiveUser(ID, Code, Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}
