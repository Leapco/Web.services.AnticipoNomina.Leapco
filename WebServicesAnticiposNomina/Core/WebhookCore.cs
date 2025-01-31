using System.Data;
using System.Text;
using System.Text.Json;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.DataBase;

namespace WebServicesAnticiposNomina.Core
{
    public class WebhookCore
    {
        private IConfiguration _configuration;
        public WebhookCore(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void SendWebhook(int idAnticipo)
        {
            WebhookModel webhookModel = new(_configuration);
            DataTable dataTable =  webhookModel.GetProcesoWebhook(idAnticipo, 1);

            foreach (DataRow row in dataTable.Rows)
            {
                string url = row["url"].ToString();
                string JsonDataWebhook = row["JsonDataWebhook"].ToString();
                ProcessingSendWebhook(url, JsonDataWebhook, idAnticipo);
            }
        }
        public bool ProcessingSendWebhook(string url, string payload, int idAnticipo)
        {
            LogsModel _logsModel = new(_configuration);
            var json = payload;
            var logRequest = new LogRequest()
            {
                Origen = "procesandoWebhook",
                Request_json = json,
                Observacion = string.Empty,
                Id_Anticipo = idAnticipo
            };

            try
            {
                using (var _httpClient = new HttpClient())
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = _httpClient.PostAsync(url, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        logRequest.Observacion = $"Webhook correctamente | Id_Anticipo : {idAnticipo}";
                        _logsModel.PostLog(logRequest);
                        return true;
                    }
                    else
                    {
                        logRequest.Observacion = $"Webhook | Id_Anticipo : {idAnticipo}";
                        _logsModel.PostLog(logRequest);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //log de error
                logRequest.Observacion = $"Error Webhook | Id_Anticipo : {idAnticipo} | Error: {ex.Message}";
                _logsModel.PostLog(logRequest);
                return false;
            }
        }
    }
}