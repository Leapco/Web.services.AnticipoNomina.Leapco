using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Response;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidateChangePasswordController : ControllerBase
    {
        public IConfiguration _configuration;

        public ValidateChangePasswordController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public ResponseModels Get([FromHeader] string Token, string ID)
        {
            ResponseModels responseModels = new();
            try
            {
                UserCore userCore = new(_configuration);
                responseModels = userCore.ValidateChangePassword(ID,Token);
            }
            catch (Exception)
            {
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}
