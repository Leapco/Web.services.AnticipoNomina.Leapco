using Microsoft.AspNetCore.Mvc;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Response;
namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RefreshDataController : ControllerBase
    {
        private IConfiguration _configuration;
        public RefreshDataController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ResponseLoginModels Get([FromHeader] string Token, string ID, int Option)
        {
            ResponseLoginModels responseModels = new();
            try
            {
                UserCore userCore = new(_configuration);
                responseModels = userCore.GetDataGeneral(ID, Option, Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}
