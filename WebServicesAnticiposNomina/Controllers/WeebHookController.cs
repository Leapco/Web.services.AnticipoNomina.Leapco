using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using WebServicesAnticiposNomina.Core;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;
using WebServicesAnticiposNomina.Models.DataBase;

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
        public string Post([FromBody] dynamic webHookRequest)
        {
            string? responseModels;
            try
            {
                ApiCobreCore apiCobreCore = new(_configuration);
                responseModels = apiCobreCore.WeebHookPayment(webHookRequest);
            }
            catch (Exception)
            {
                responseModels = "500";
            }
            return responseModels;
        }

        [HttpGet]
        public string Get()
        {
            try
            {
                AdvanceModel advanceModel = new(_configuration);
                AdvanceRequest advanceRequest = new();
                DataTable dataWebHook = advanceModel.PostAdvance(advanceRequest, 14);

                List<WebHookRequest> webHookRequestList = new List<WebHookRequest>();
                foreach (DataRow row in dataWebHook.Rows)
                {
                    string json = row["Request_json"].ToString();
                    WebHookRequest webHookRequest = JsonConvert.DeserializeObject<WebHookRequest>(json);
                    webHookRequestList.Add(webHookRequest);
                }

                foreach (WebHookRequest webHookRequest in webHookRequestList)
                {
                    Thread.Sleep(3000);
                    ApiCobreCore apiCobreCore = new(_configuration);
                    _ = apiCobreCore.WeebHookPayment(webHookRequest);
                }
                return "WebHook registrados";
            }
            catch (Exception)
            {
                return "Hubo un error";
            }
            
        }
    }
}