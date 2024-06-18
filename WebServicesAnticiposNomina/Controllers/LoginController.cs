using Microsoft.AspNetCore.Mvc;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost(Name = "Login")]
        public ResponseLoginModels Post([FromBody] LoginRequest loginRequest)
        {
            ResponseLoginModels result = new();
            try
            {
                LoginCore loginCore = new(_configuration);
                result = loginCore.Login(loginRequest);
                return result;
            }
            catch (Exception ex)
            {
                result.CodeResponse = "500";
                result.MessageResponse = ex.Message;
                return result;
            }
            
        }
    }
}